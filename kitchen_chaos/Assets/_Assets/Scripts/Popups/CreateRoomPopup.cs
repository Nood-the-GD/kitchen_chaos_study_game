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
    public GameObject text;
    [HideInInspector]public StageData selectStage = null;

    public GameObject stageParent;
    const string CMD_SWITCH_STAGE = "CmdSwitchStage"; 



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
        PhotonManager.s.onPlayerEnteredRoom += OnPlayerEnterRoom;
        PhotonManager.s.onPlayerLeftRoom += OnPlayerLeaveRoom;
        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
    }

    void OnCallAnyCmdFunction(CmdOrder order){
        if(order.reciver != nameof(CreateRoomPopup)) return;

        if(order.functionName == CMD_SWITCH_STAGE){
            RpcSwitchStage((int)order.data[0]);
        }
    }

    

    void OnPlayerEnterRoom(Photon.Realtime.Player player){
        RefeshUI();

        
        if(PhotonNetwork.IsMasterClient){
            CmdSwitchStage(selectStage.levelId);
        }

    }

    void OnPlayerLeaveRoom(Photon.Realtime.Player player){
        RefeshUI();
    }

    protected override void OnDisable(){
        base.OnDisable();
        PhotonManager.s.onJoinRoom -= RefeshUI;
        PhotonManager.s.onLeaveRoom -= OnLeaveRoom;
        PhotonManager.s.onPlayerEnteredRoom -= OnPlayerEnterRoom;
        PhotonManager.s.onPlayerLeftRoom -= OnPlayerLeaveRoom;
        PhotonManager.s.onCallAnyCmdFunction -= OnCallAnyCmdFunction;

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
            Debug.Log("Setup stage "+stageData.levelId);
            stageUIs[i].SetData(stageData, OnSwitchStage);
        }

        foreach(var i in stageUIs){
            
            i.gameObject.SetActive(true);
            i.transform.DOScale(Vector3.zero, 0.25f).From().SetEase(Ease.OutBack);
            await Task.Delay(100);
            
        }

        CmdSwitchStage(stages[0].levelId);
    }

    void OnSwitchStage(StageData stageData){
        CmdSwitchStage(stageData.levelId);
    }

    void CmdSwitchStage(int id){
        Debug.Log("CmdSwitchStage " + id);
        var order = new CmdOrder(nameof(CreateRoomPopup),CMD_SWITCH_STAGE, id);
        PhotonManager.s.CmdCallFunction(order);

    }

    void RpcSwitchStage(int id){
        Debug.Log("RpcSwitchStage " + id);
        var stageData = GameData.s.GetStage(id);
        foreach(var stage in stageUIs){
            if(stage.stageData.levelId == stageData.levelId)
                stage.SetSelect();
            else
                stage.Unselect();
        }

        previewImage.sprite = stageData.previewImage;
        selectStage = stageData;

        var rootScale = 2.225f;
        
        previewImageParent.transform.DOScale(rootScale, 0.25f).From(0).SetEase(Ease.OutBack);
    }

    
    
    
    void RefeshUI(){

        foreach(var i in playerUIElements){
            i.gameObject.SetActive(false);
        }

        var roomPlayer = PhotonNetwork.PlayerList.Length;
        
        for(int i = 0; i< roomPlayer; i++){
            playerUIElements[i].gameObject.SetActive(true);
            //playerUIElements[i].transform.localScale = Vector3.zero;
            playerUIElements[i].transform.DOScale(0.3f, 0.25f).From(0).SetEase(Ease.OutBack);
            
            playerUIElements[i].SetData(PhotonNetwork.PlayerList[i]);
        }

        var activateStartButton = roomPlayer == 2 && PhotonNetwork.IsMasterClient;
        startButton.gameObject.SetActive(activateStartButton);
        text.SetActive(!activateStartButton);
        roomName.text = "ID: "+PhotonNetwork.CurrentRoom.Name;
    }

    


}
