using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialData : MonoBehaviour
{
    private int skipTutorialNumber = 0;
    private int confirmTutorialNumber = 0;
    public static TutorialData Instance;

    void Awake()
    {
        skipTutorialNumber = 0;
        Instance = this;
    }
    void Start()
    {
        if(PhotonManager.s == null){
            Debug.LogError("PhotonManager is null");
            return;
        }
        PhotonManager.s.onJoinRoom += OnJoinRoom;
    }

    private void OnJoinRoom()
    {
        skipTutorialNumber = 0;
    }

    public void SkipTutorial(bool value)
    {
        if(value)
        {
            skipTutorialNumber++;
        }
        else
        {
            skipTutorialNumber--;
        }
        if (skipTutorialNumber < 0) skipTutorialNumber = 0;
    }
    public void ConfirmTutorial()
    {
        confirmTutorialNumber++;
        if(SectionData.s.isSinglePlay)
        {
            confirmTutorialNumber++;
        }
    }

    public void ClearConfirmTutorial()
    {
        confirmTutorialNumber = 0;
    }

    public int GetSkipTutorialNumber()
    {
        return skipTutorialNumber;
    }
    public int GetConfirmTutorialNumber()
    {
        return confirmTutorialNumber;
    }
}
