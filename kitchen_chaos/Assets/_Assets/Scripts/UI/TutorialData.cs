using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialData : MonoBehaviour
{
    private int skipTutorialNumber = 0;
    private int confirmTutorialNumber = 0;
    public static TutorialData Instance;

    void Start()
    {
        skipTutorialNumber = 0;
        Instance = this;
        if (PhotonManager.s == null)
        {
            Debug.LogError("PhotonManager is null");
            return;
        }
        PhotonManager.s.onJoinRoom += OnJoinRoom;
        PhotonManager.s.onConnectToServer += OnConnectToServer;
    }

    private void OnConnectToServer()
    {
        if (UserData.IsFirstTutorialDone == false)
        {
            SectionData.s.isSinglePlay = true;
            PhotonManager.s.onCreatedRoom += OnCreatedRoom;
            PhotonNetwork.CreateRoom("tutorial: "+ UserData.currentUser.uid);
        }
        PhotonManager.s.onConnectToServer -= OnConnectToServer;
    }

    private void OnCreatedRoom()
    {
        LoadTutorialLevel();
        PhotonManager.s.onCreatedRoom -= OnCreatedRoom;
    }
    private void LoadTutorialLevel()
    {
        StartCoroutine(LoadTutorialLevelCoroutine());
    }

    private IEnumerator LoadTutorialLevelCoroutine()
    {
        while (!PhotonManager.s.isDoneInitServerList)
        {
            yield return null;
        }

        GameManager.levelId = 0;
        PhotonNetwork.LoadLevel("Level_tutorial");
    }

    

    private void OnJoinRoom()
    {
        skipTutorialNumber = 0;
    }

    public void SkipTutorial(bool value)
    {
        if (value)
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
        if (SectionData.s.isSinglePlay)
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
