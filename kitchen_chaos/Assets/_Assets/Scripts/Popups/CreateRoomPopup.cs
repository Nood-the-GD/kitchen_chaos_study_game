using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;

public class CreateRoomPopup : BasePopup<CreateRoomPopup>{
    public TextMeshProUGUI roomName;
    public List<PlayerUIElement> playerUIElements;
    public GameObject startButton;
    public PhotonView photonView;
    public List<StageUI> stageUIs;
    public Image previewImage;
    public GameObject previewImageParent;
    [HideInInspector]public StageData selectStage = null;

    public GameObject stageParent;



    public void OnStartButtonClick(){
        PhotonNetwork.LoadLevel(Loader.Scene.GameScene.ToString());
    }

    public void OnExitButtonClick(){
        PhotonNetwork.LeaveRoom();
    }


    void Start(){
        RefeshUI();
        SetupStages();
        stageParent.SetActive(PhotonNetwork.IsMasterClient);
        
    }

    //public void SelectLevel
    protected override void OnEnable(){
        base.OnEnable();
        PhotonManager.s.onJoinRoom += RefeshUI;
        PhotonManager.s.onLeaveRoom += OnLeaveRoom;

    }

    protected override void OnDisable(){
        base.OnDisable();
        PhotonManager.s.onJoinRoom -= RefeshUI;
        PhotonManager.s.onLeaveRoom -= OnLeaveRoom;
    }

    void OnLeaveRoom(){
        HidePopup();
    }

    
    async void SetupStages(){
        var stages = GameData.s.stages;

        foreach(var i in stageUIs){
            i.gameObject.SetActive(false);
        }

        for(int i = 0; i< stages.Count; i++){
            var stageData = stages[i];
            stageUIs[i].SetData(stageData, OnSwitchStage);
        }

        foreach(var i in stageUIs){
            
            i.gameObject.SetActive(true);
            i.transform.DOScale(Vector3.zero, 0.25f).From().SetEase(Ease.OutBack);
            await Task.Delay(100);
            
        }




    }

    void OnSwitchStage(StageData stageData){
        CmdSwitchStage(stageData.levelId);
    }

    void CmdSwitchStage(int id){
        photonView.RPC("RpcSwitchStage", RpcTarget.All, id);

    }

    [PunRPC]
    void RpcSwitchStage(int id){
        var stageData = GameData.s.GetStage(id);
        foreach(var stage in stageUIs){
            if(stage.stageData.levelId == stageData.levelId)
                stage.SetSelect();
            else
                stage.Unselect();
        }

        previewImage.sprite = stageData.previewImage;
        selectStage = stageData;

        var rootScale = 0.7f;
        previewImageParent.transform.localScale = new Vector3(rootScale,rootScale,rootScale);
        previewImageParent.transform.DOScale(1, 0.25f).From().SetEase(Ease.OutBack);
    }

    
    
    
    void RefeshUI(){

        foreach(var i in playerUIElements){
            i.gameObject.SetActive(false);
        }

        var roomPlayer = PhotonNetwork.CountOfPlayersInRooms;
        for(int i = 0; i< roomPlayer; i++){
            playerUIElements[i].gameObject.SetActive(true);
            playerUIElements[i].SetData(PhotonNetwork.PlayerList[i]);
        }

        var activateStartButton = roomPlayer == 2 && PhotonNetwork.IsMasterClient;
        startButton.gameObject.SetActive(activateStartButton);
        roomName.text = "ID:"+PhotonNetwork.CurrentRoom.Name;
    }

    


}
