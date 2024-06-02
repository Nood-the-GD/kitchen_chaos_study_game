using UnityEngine;
using Photon.Pun;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;
public class SelectModePopup : BasePopup<SelectModePopup>
{
    [SerializeField] private Button _singlePlayBtn, _randomMatchBtn, _findRoomBtn, _createRoomBtn;
     
    private void Start() {
        PhotonManager.s.onJoinRandomRoomFailed += OnJoinRandomRoomFailed;
        _singlePlayBtn.onClick.AddListener(SinglePlay);
        _randomMatchBtn.onClick.AddListener(RandomMatch);
        _findRoomBtn.onClick.AddListener(FindRoom);
        _createRoomBtn.onClick.AddListener(CreateRoom);
    }

    private void OnJoinRandomRoomFailed(string obj)
    {
        NoRoomFoundPopup.ShowPopup();
    }

    private void SinglePlay()
    {
        SectionData.s.isSinglePlay = true;
        PhotonNetwork.CreateRoom(RandomStringGenerator.GenerateRandomString(7));
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
