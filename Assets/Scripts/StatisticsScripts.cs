﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite; using System.Data;
using UnityEngine.UI;

public class StatisticsScripts : MonoBehaviour {
    #region Variables

	public string TestTypeSearch="";
	public string StuNoSearch="";

	private GameObject GridWithRows;
	private GameObject StartingRow;

    private string[] Testnames;
    private int[] StudentNumbers;
	private List<int[]> Questions = new List<int[]>();
	private int numberOfResults = 0;
	
		
    #endregion

	#region Unity Callbacks
	
    // Use this for initialization
    void Start ()
    {
	    GridWithRows = GameObject.Find("GridWithRows");
	    StartingRow = GameObject.Find("StartingRow");
	    getData();
	    Display();
    }
	
    // Update is called once per frame
    void Update () {
		
    }
	
	IEnumerator Example()
	{
		
		yield return new WaitForSeconds(1);
		GameObject.Find("HorizontalScrollBar").GetComponent<Scrollbar>().value = 0;
	}
	
	#endregion
	
	#region Functions

    private void getData()
    {
        string conn = "URI=file:" + Application.dataPath + "/Database/Database.db"; //Path to database.
		
        IDbConnection dbconn;
        dbconn = (IDbConnection) new SqliteConnection(conn);
        dbconn.Open(); //Open connection to the database.
	    IDbCommand dbcmd = dbconn.CreateCommand();
	    IDbCommand dbcmd2 = dbconn.CreateCommand();

	    string sqlQuery = "SELECT COUNT(*) FROM Test";
	    //string sqlQuery = "SELECT COUNT(*) FROM Test where TestType Like '" + TestTypeSearch+"' and StuNo = "+StuNoSearch;
	    
	    dbcmd.CommandText = sqlQuery;
	    IDataReader reader = dbcmd.ExecuteReader();

	    reader.Read();
	    Debug.Log(reader.GetInt32(0));

	    numberOfResults = reader.GetInt32(0);

	    Testnames = new string[numberOfResults];
	    StudentNumbers = new int[numberOfResults];
        
	    //sqlQuery = "SELECT TestType,StuNo,ifnull(q1,-1)q1,ifnull(q2,-1)q2,ifnull(q3,-1)q3,ifnull(q4,-1)q4,ifnull(q5,-1)q5,ifnull(q6,-1)q6,ifnull(q7,-1)q7,ifnull(q8,-1)q8,ifnull(q9,-1)q9,ifnull(q10,-1)q10 FROM Test";
	    //sqlQuery = "Select * From Test where TestType Like '" + TestTypeSearch+"' and StuNo = "+StuNoSearch;
	    sqlQuery = "Select * From Test";
	    dbcmd2.CommandText = sqlQuery;
        reader = dbcmd2.ExecuteReader();

	    var counter = 0;

	    
	    while(reader.Read())
	    {
		    var readerFieldCount = reader.FieldCount;

		    Testnames[counter] = reader.GetString(0);
		    StudentNumbers[counter] = reader.GetInt32(1);

		    int[] TempQuestionArray = new int[(readerFieldCount-2)];

		    for (var i = 2; i < readerFieldCount; i++)
		    {
			    print(reader.GetInt32(i));
			    if (reader.GetInt32(i) == -1) continue;
			    
				    TempQuestionArray[(i-2)] = reader.GetInt32(i);
			    
		    }
		    Questions.Add(TempQuestionArray);

		    counter++;

	    }
		
        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
	    dbcmd2.Dispose();
	    dbcmd2 = null;
        dbconn.Close();
        dbconn = null;
    }

	private void Display()
	{
		var i = 0;
		var MaxNumberOfQuestions = 0;
		for (i = 0; i < numberOfResults; i++)
		{
			if (Questions[i].Length>= MaxNumberOfQuestions)
			{
				MaxNumberOfQuestions = Questions[i].Length;
			}
		}

		for (i = 0; i < MaxNumberOfQuestions; i++)
		{
			GameObject NewObj = (GameObject) Instantiate(Resources.Load("Question"), StartingRow.transform);
			NewObj.transform.GetChild(0).GetComponent<Text>().text = "Soru" + (i + 1);

		}
		
		for (i = 0; i < Testnames.Length; i++)
		{
			GameObject NewRow = (GameObject) Instantiate(Resources.Load("RowWithColumns"), GridWithRows.transform);
			
			GameObject NewTestTypeObject = (GameObject) Instantiate(Resources.Load("TestType"), NewRow.transform);
			NewTestTypeObject.transform.GetChild(0).GetComponent<Text>().text = Testnames[i];
			
			GameObject NewStuNo = (GameObject) Instantiate(Resources.Load("StuNo"), NewRow.transform);
			NewStuNo.transform.GetChild(0).GetComponent<Text>().text = StudentNumbers[i].ToString();

			for (var j = 0; j < Questions[i].Length; j++)
			{
				if (Questions[i][j] == -1) continue;
				GameObject NewQuestionObject = (GameObject) Instantiate(Resources.Load("Question"), NewRow.transform);
				NewQuestionObject.transform.GetChild(0).GetComponent<Text>().text = Questions[i][j].ToString();
			}
		}

		StartCoroutine(Example());
	}

	public void Search()
	{
		getData();
		Display();
	}
	
	#endregion
}