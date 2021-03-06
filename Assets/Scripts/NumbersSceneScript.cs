﻿using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.UI;
using Mono.Data.Sqlite; using System.Data;
using System.IO;

public class NumbersSceneScript : MonoBehaviour {

	#region Variables

    public GameObject questionTextObject, ShowPictureObject, restartObject, testStartObject, goBackObject,Racoon, RacoonText,StarPanel ;
    public GameObject[] TestPictureObjects,Stars;
	public AudioSource ApplauseAudioSource;
	
	public AudioClip[] IdentificationAudioClips, QuestionAudioClips, congratsAudioClips;
	private AudioSource AudioSource;
	private bool noAudioPlaying = true;
	private GameObject RacoonHelpObject;
    private int PictureCounter,randomInt;
	private int[] FailCounter = new int[9];
	private string TestName = "rakamlar";
	private string[] NumbersInTextForm = {"bir", "iki", "üç", "dört", "beş","altı", "yedi", "sekiz", "dokuz"};
	private string conn;
	private Coroutine co;
	private float[] AnswerTimes = new float[9];
	private float passedTime;
	
    #endregion
	
    #region Unity Callbacks
    
    // Use this for initialization
    void Start ()
    {
	    RacoonHelpObject = (GameObject)Instantiate(Resources.Load("RacoonHelp"));
	    PictureCounter = 0;
	    
	    foreach (var t in TestPictureObjects)
	    {
		    t.tag = "trueAnswer";
	    }
	    
	    for (var i = 0; i < FailCounter.Length; i++)
	    {
		    FailCounter[i] = 0;
	    }
	    RacoonText.GetComponent<Text>().text = NumbersInTextForm[0];
	    
	    AudioSource = gameObject.GetComponent<AudioSource>();
        
	    congratsAudioClips = Resources.LoadAll<AudioClip>("Sound/Congrats");
	    ApplauseAudioSource.clip = (AudioClip) Resources.Load("Sound/applause");
	    showNumbers();
    }
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			StarAnimationScript.counp = 0;
			SceneManager.LoadScene("MainScene");
		}
	}
	
	IEnumerator StartRacoonHelpCounter(int i)
	{
		yield return new WaitForSeconds(6f);
		RacoonHelpObject.GetComponent<RectTransform>().SetParent(TestPictureObjects[i].GetComponent<RectTransform>());
		RacoonHelpObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100f);
		RacoonHelpObject.gameObject.SetActive(true);
		RacoonHelpObject.GetComponent<RectTransform>().localScale = new Vector3(1.0f,1.0f,0f);
	}
	
	IEnumerator WaitUntilQuestion()
	{
		yield return new WaitForSeconds(AudioSource.clip.length);
		passedTime = Time.time;
	}
	
	
	IEnumerator IdentifySound()
	{
		noAudioPlaying = false;
        
		AudioSource.clip = IdentificationAudioClips[PictureCounter-1];
		AudioSource.Play();
		yield return new WaitForSeconds(AudioSource.clip.length);
        
		showNumbers();
		noAudioPlaying = true;
	}
    
	IEnumerator CongratsSound(int i)
	{
		if (!TestPictureObjects[i].CompareTag("trueAnswer")) {
			FailCounter[PictureCounter-1]++;
			TestPictureObjects[i].GetComponent<Text>().color  = new Color32(0,0,0,100);
			
			var tempNumber = 0;
			if (i == 0)
				tempNumber = 1;
			StopCoroutine(co);
			RacoonHelpObject.GetComponent<RectTransform>().SetParent(TestPictureObjects[tempNumber].GetComponent<RectTransform>());
			RacoonHelpObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100f);
			RacoonHelpObject.gameObject.SetActive(true);
			RacoonHelpObject.GetComponent<RectTransform>().localScale = new Vector3(1.0f,1.0f,0f);
			
			yield break;
		}
		
		passedTime = Time.time - passedTime;
		AnswerTimes[PictureCounter-1] = passedTime;
		
		StopCoroutine(co);
		RacoonHelpObject.SetActive(false);
		noAudioPlaying = false;
		
		if (PictureCounter < 9)
			gameObject.GetComponent<StarAnimationScript>().StarFunction();

		yield return new WaitUntil(() => gameObject.GetComponent<StarAnimationScript>().getAPanelFinished() == true);
        
		AudioSource.clip = congratsAudioClips[UnityEngine.Random.Range(0,5)];
		AudioSource.Play();
		ApplauseAudioSource.Play();
		yield return new WaitForSeconds(AudioSource.clip.length);
		gameObject.GetComponent<StarAnimationScript>().deactivateAPanel();
		
		if (PictureCounter >= 9)
		{
			gameObject.GetComponent<StarAnimationScript>().StartAnimation();
			
			questionTextObject.SetActive(false);
			foreach(var t in TestPictureObjects)
				t.SetActive(false);
			
			SendDataToDB();
			AddStar();
			StarPanel.SetActive(true);
			restartObject.SetActive(true);
			testStartObject.SetActive(true);
			goBackObject.SetActive(true);
			questionTextObject.SetActive(false);
			noAudioPlaying = true;
			yield break;
		}
		foreach ( var t in TestPictureObjects)
			t.GetComponent<Text>().color  = new Color32(0,0,0,255);
        
		testNumbers();
		noAudioPlaying = true;
	}
    
    #endregion
    
    #region Functions

    public void RestartScene()
    {
        SceneManager.LoadScene("NumbersScene");
    }
	
	public void PlaySound()
	{
		if(noAudioPlaying)
			StartCoroutine(IdentifySound());
	}
    
	public void PlayCongrats(int i)
	{
		if(noAudioPlaying && !AudioSource.isPlaying)
			StartCoroutine(CongratsSound(i));
	}

	private void showNumbers()
	{
		if (PictureCounter < 9)
		{
			RacoonText.GetComponent<Text>().text = NumbersInTextForm[PictureCounter];
			PictureCounter++;
			ShowPictureObject.GetComponent<Text>().text = PictureCounter.ToString();
		}
		else
		{
			if (StarAnimationScript.counp < 8)
				testStartObject.SetActive(false);
			else
				testStartObject.SetActive(true);
			
			ShowPictureObject.SetActive(false);
			Racoon.SetActive(false);
			StarPanel.SetActive(true);
			if (StarAnimationScript.counp < 5)
				StarAnimationScript.counp++;
			AddStar();
			restartObject.SetActive(true);
			goBackObject.SetActive(true);
			PictureCounter = 0;
		}
	}

	public void goToMainMenu()
	{
		SceneManager.LoadScene("MainScene");
	}

	public void startTest()
	{
		
			restartObject.SetActive(false);
			testStartObject.SetActive(false);
			goBackObject.SetActive(false);

			foreach (var t in TestPictureObjects)
			{
				t.SetActive(true);
			}
			
			StarPanel.SetActive(false);
			for(int i=0;i<StarAnimationScript.counp;i++)
				Stars[i].SetActive(false);

			testNumbers();
		
	}

	private void testNumbers()
	{
		goBackObject.SetActive(false);
		questionTextObject.SetActive(true);
		
		AudioSource.clip = QuestionAudioClips[PictureCounter];
		AudioSource.Play();
		StartCoroutine(WaitUntilQuestion());

		questionTextObject.GetComponent<Text>().text = "Hangisi " + NumbersInTextForm[PictureCounter] +" göster.";
		PictureCounter++;
		randomInt = UnityEngine.Random.Range(1, 10);
		
		while (randomInt == PictureCounter)
		{
			randomInt = UnityEngine.Random.Range(1, 10);
		}

		var falseAnswer = randomInt;

		randomInt = UnityEngine.Random.Range(0, 2);

		switch (randomInt)
		{
			case 0:
				TestPictureObjects[randomInt].tag = "trueAnswer";
				TestPictureObjects[randomInt].GetComponent<Text>().text = PictureCounter.ToString();
				
				TestPictureObjects[1].GetComponent<Text>().text = falseAnswer.ToString();
				TestPictureObjects[1].tag = "falseAnswer";
				
				co = StartCoroutine(StartRacoonHelpCounter(randomInt));

				break;
			case 1:
				TestPictureObjects[randomInt].tag = "trueAnswer";
				TestPictureObjects[randomInt].GetComponent<Text>().text = PictureCounter.ToString();
				
				TestPictureObjects[0].GetComponent<Text>().text = falseAnswer.ToString();
				TestPictureObjects[0].tag = "falseAnswer";
				
				co = StartCoroutine(StartRacoonHelpCounter(randomInt));
				break;
			default:
				Debug.Log("Unexpected randomint");
				break;
		}
	}
	
	public void SendDataToDB()
	{
		IDbConnection dbconn = connectToDB();
		dbconn.Open(); //Open connection to the database.

		IDbCommand dbcmd = dbconn.CreateCommand();
		
		string sqlQuery = "INSERT INTO Test (TestType,StuNo,q1,q2,q3,q4,q5,q6,q7,q8,q9) values ('"+TestName+"',"+PlayerPrefs.GetInt("StuNumber")+","+FailCounter[0]+","+FailCounter[1]+","+FailCounter[2]+","+FailCounter[3]+","+FailCounter[4]+","+FailCounter[5]+","+FailCounter[6]+","+FailCounter[7]+","+FailCounter[8]+")";
		
		dbcmd.CommandText = sqlQuery;
		IDataReader reader = dbcmd.ExecuteReader();
		
		IDbCommand dbcmd2 = dbconn.CreateCommand();
		sqlQuery = "INSERT INTO TestTimes (TestType,StuNo,q1,q2,q3,q4,q5,q6,q7,q8,q9) values ('"+TestName+"',"+PlayerPrefs.GetInt("StuNumber")+","+AnswerTimes[0]+","+AnswerTimes[1]+","+AnswerTimes[2]+","+AnswerTimes[3]+","+AnswerTimes[4]+","+AnswerTimes[5]+","+AnswerTimes[6]+","+AnswerTimes[7]+","+AnswerTimes[8]+")";
		dbcmd2.CommandText = sqlQuery;
		reader = dbcmd2.ExecuteReader();
        
		dbcmd2.Dispose();
		dbcmd2 = null;
		reader.Close();
		reader = null;
		dbcmd.Dispose();
		dbcmd = null;
		dbconn.Close();
		dbconn = null;
	}
	
	private IDbConnection connectToDB()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			conn = Application.persistentDataPath + "/Database.db";

			if(!File.Exists(conn)){
				WWW loadDB = new WWW("jar:file://" + Application.dataPath+ "!/assets/Database.db");
			
				while(!loadDB.isDone){}

				File.WriteAllBytes(conn,loadDB.bytes);
			}

		}
		else
		{
			conn =Application.dataPath + "/StreamingAssets/Database.db";
		}
		return (IDbConnection)new SqliteConnection("URI=file:" + conn);
	}

	
	public void AddStar()
	{
		for(int i=0;i<StarAnimationScript.counp;i++)
			Stars[i].SetActive(true);
	}
	
    
    #endregion
}
