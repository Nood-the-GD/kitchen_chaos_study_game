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

    // This event is raised when the tutorial confirmation count has reached the threshold.
    public static event Action OnTutorialSyncConfirmed;

    void Start()
    {
        skipTutorialNumber = 0;
        Instance = this;
        if (PhotonManager.s == null)
        {
            Debug.LogError("PhotonManager is null");
            return;
        }
        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
        PhotonManager.s.onJoinRoom += OnJoinRoom;
        PhotonManager.s.onConnectToServer += OnConnectToServer;
    }

    void OnDestroy()
    {
        PhotonManager.s.onCallAnyCmdFunction -= OnCallAnyCmdFunction;
    }

    private void OnConnectToServer()
    {
        if (UserData.IsFirstTutorialDone == false)
        {
            SectionData.s.isSinglePlay = true;
            PhotonManager.s.onCreatedRoom += OnCreatedRoom;
            PhotonNetwork.CreateRoom("tutorial: " + UserData.currentUser.uid);
        }
        PhotonManager.s.onConnectToServer -= OnConnectToServer;
    }

    public void CmdConfirmTutorial()
    {
        var order = new CmdOrder(nameof(TutorialData), nameof(ConfirmTutorial));
        PhotonManager.s.CmdCallFunction(order);
    }

    private void OnCallAnyCmdFunction(CmdOrder order)
    {
        if (order.receiver != nameof(TutorialData)) return;

        if (order.functionName == nameof(ConfirmTutorial))
        {
            ConfirmTutorial();
        }
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

        // In singleplay mode we count using double increment so that it immediately
        // triggers the confirmation threshold.
        if (SectionData.s.isSinglePlay)
        {
            confirmTutorialNumber++;
        }

        if (confirmTutorialNumber >= 2)
        {
            // When the required number of confirmations is reached,
            // the data layer tells everyone to proceed.
            OnTutorialSyncConfirmed?.Invoke();
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