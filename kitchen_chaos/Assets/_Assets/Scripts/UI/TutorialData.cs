using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialData : MonoBehaviour
{
    private int skipTutorialNumber = 0;
    public static TutorialData Instance;

    void Awake()
    {
        skipTutorialNumber = 0;
        Instance = this;
    }
    void OnEnable()
    {
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
    public int GetSkipTutorialNumber()
    {
        return skipTutorialNumber;
    }
}
