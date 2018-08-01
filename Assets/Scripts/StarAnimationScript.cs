﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public class StarAnimationScript : MonoBehaviour
{

    public GameObject StarEndAnimation, APanel;

    private int starCounter = 0;
    private GameObject[] allChildren;
    private bool APanelFinished;

    public void StartAnimation()
    {
        StarEndAnimation.SetActive(true);
        StarEndAnimation.GetComponent<Animator>().Play("StarDiving",-1,0f);
        StarEndAnimation.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Sound/StarFallSound"),0.8f);
    }

    public void EndAnimation()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public void StarFunction(GameObject obj)
    {
        obj.SetActive(false);
        starCounter++;
        APanel.GetComponent<AudioSource>().Play();
        if (starCounter >= 4)
        {
            APanel.transform.GetChild(1).gameObject.SetActive(false);
            starCounter = 0;
            APanelFinished = true;
        }
            
    }
	
    public void StarFunction()
    {
        APanelFinished = false;
        APanel.SetActive(true);
        APanel.transform.GetChild(1).gameObject.SetActive(true);
        ActivatePanelsChildren();
    }

    public void ActivatePanelsChildren()
    {
        allChildren = new GameObject[APanel.transform.childCount];
        var i = 0;
        foreach (Transform child in APanel.transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }
        
        foreach (GameObject child in allChildren)
        {
            child.SetActive(true);
        }
    }

    public void deactivateAPanel()
    {
        APanel.SetActive(false);
    }

    public bool getAPanelFinished()
    {
        return APanelFinished;
    }
	
}