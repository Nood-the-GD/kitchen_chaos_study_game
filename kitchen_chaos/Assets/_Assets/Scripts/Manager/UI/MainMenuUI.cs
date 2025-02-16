using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playBtn; 
    [SerializeField] private GameObject loadingGO;
    [SerializeField] private PhotonManager photonManager;
    [SerializeField] private Text username;

    private void Awake()
    {
        playBtn.onClick.AddListener(() => {
            //Loader.Load(Loader.Scene.GameScene);
            SelectModePopup.ShowPopup();
        });
        Time.timeScale = 1f;
        playBtn.gameObject.GetComponent<Image>().color = Color.gray;
        playBtn.interactable = false;
       
    }


    void OnEnable()
    {
        UserData.currentUser.OnUpdateUserName += OnUpdateUserName;
        photonManager.onConnectToServer += PhotonManager_OnConnectToServerHandler;
    }

    void OnUpdateUserName(){
        username.text = "Username: "+ UserData.currentUser.username;
    }
    void OnDisable()
    {
        UserData.currentUser.OnUpdateUserName -= OnUpdateUserName;
        photonManager.onConnectToServer -= PhotonManager_OnConnectToServerHandler;
    }
    void Start(){
        if(PhotonManager.s == null || !PhotonManager.s.isServerConnected)
        {
            playBtn.gameObject.GetComponent<Image>().color = Color.gray;
            playBtn.interactable = false;
        }

        username.text = "Username: "+ UserData.currentUser.username;
        if(Application.isEditor){
            username.text += "\n userId: " + UserData.currentUser.uid;
        }
    }

    private void PhotonManager_OnConnectToServerHandler()
    {
        playBtn.gameObject.GetComponent<Image>().color = Color.green;
        playBtn.interactable = true;
        loadingGO.SetActive(false);
    }

    public void OnSettingBtnClick(){
        SettingPopup.ShowPopup();
    }
}
