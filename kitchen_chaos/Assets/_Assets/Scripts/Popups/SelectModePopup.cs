using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using Sirenix.OdinInspector;
public class SelectModePopup : BasePopup<SelectModePopup>
{
    private void Start() {
        PhotonManager.s.onJoinRandomRoomFailed += OnJoinRandomRoomFailed;
    }
    private void OnJoinRandomRoomFailed(string obj)
    {
        NoRoomFoundPopup.ShowPopup();
    }

    public void RandomMatch(){
        
        PhotonNetwork.JoinRandomRoom();
    }

    public void FindRoom(){
        FindRoomPopup.ShowPopup();
    }

    protected override void OnEnable(){
        base.OnEnable();
        PhotonManager.s.onJoinRoom += EnterRoom;
    }

    protected override void OnDisable(){
        base.OnDisable();
        PhotonManager.s.onJoinRoom -= EnterRoom;
        PhotonManager.s.onJoinRandomRoomFailed -= OnJoinRandomRoomFailed;
    }

    void EnterRoom(){
        CreateRoomPopup.ShowPopup();
        HidePopup();
    }

    public void CreateRoom(){

        PhotonNetwork.CreateRoom(RandomStringGenerator.GenerateRandomString(7));

       
    }



}
