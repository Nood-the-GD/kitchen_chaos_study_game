using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;

public class CreateRoomPopup : BasePopup<CreateRoomPopup>{
    public TextMeshProUGUI roomName;
    public List<PlayerUIElement> playerUIElements;
    public GameObject startButton;
    public PhotonView photonView;
    public List<StageUI> stageUIs;
    public Image previewImage;
    public GameObject previewImageParent;
    public GameObject text;
    [SerializeField] private StarController currentLevelStarController;
    [HideInInspector]public StageData selectStage = null;

    public GameObject stageParent;
    const string CMD_SWITCH_STAGE = "CmdSwitchStage"; 
    const string CMD_NEXT_SCENE = "CmdNextScene";
    int currentSceneId = 0;

    [Button("Move to GameScene")]
    public void OnStartButtonClick(){
        //PhotonNetwork.AutomaticallySyncScene = true;
        var order = new CmdOrder(nameof(CreateRoomPopup),CMD_NEXT_SCENE);
        PhotonManager.s.CmdCallFunction(order);
    }



    public void OnExitButtonClick(){
        PhotonNetwork.LeaveRoom();
    }


    void Start(){
        currentSceneId = 0;
        RefreshUI();
        SetupStages();
        stageParent.SetActive(PhotonNetwork.IsMasterClient);
        
    }

    //public void SelectLevel
    protected override void OnEnable(){
        base.OnEnable();
        PhotonManager.s.onJoinRoom += RefreshUI;
        PhotonManager.s.onLeaveRoom += OnLeaveRoom;
        PhotonManager.s.onPlayerEnteredRoom += OnPlayerEnterRoom;
        PhotonManager.s.onPlayerLeftRoom += OnPlayerLeaveRoom;
        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
    }

    void OnCallAnyCmdFunction(CmdOrder order){
        if(order.receiver != nameof(CreateRoomPopup)) return;

        if(order.functionName == CMD_SWITCH_STAGE){
            RpcSwitchStage((int)order.data[0], (int)order.data[1]);
        }
        if(order.functionName == CMD_NEXT_SCENE){
            GameManager.levelId = currentSceneId;
            PhotonNetwork.LoadLevel(GameData.s.GetStage(currentSceneId).sceneName);
        }
    }

    

    void OnPlayerEnterRoom(Photon.Realtime.Player player){
        RefreshUI();

        
        if(PhotonNetwork.IsMasterClient){
            CmdSwitchStage(selectStage.levelId, selectStage.star);
        }

    }

    void OnPlayerLeaveRoom(Photon.Realtime.Player player){
        RefreshUI();
    }

    protected override void OnDisable(){
        base.OnDisable();
        PhotonManager.s.onJoinRoom -= RefreshUI;
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

        CmdSwitchStage(stages[0].levelId, stages[0].star);
    }

    void OnSwitchStage(StageData stageData){
        CmdSwitchStage(stageData.levelId, stageData.star);
    }

    void CmdSwitchStage(int id, int star){
        Debug.Log("CmdSwitchStage " + id);
        var order = new CmdOrder(nameof(CreateRoomPopup),CMD_SWITCH_STAGE, id, star);
        PhotonManager.s.CmdCallFunction(order);
    }

    void RpcSwitchStage(int id, int star){
        Debug.Log("RpcSwitchStage " + id);
        currentSceneId = id;
        var stageData = GameData.s.GetStage(id);
        foreach(var stage in stageUIs){
            if(stage.stageData.levelId == stageData.levelId)
                stage.SetSelect();
            else
                stage.Unselect();
        }

        currentLevelStarController.ShowStar(star);
        previewImage.sprite = stageData.previewImage;
        selectStage = stageData;

        var rootScale = 2.225f;
        
        previewImageParent.transform.DOScale(rootScale, 0.25f).From(0).SetEase(Ease.OutBack);
    }

    
    
    
    void RefreshUI(){

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

        var enoughPlayer = roomPlayer == 2;
        var activateStartButton = enoughPlayer && PhotonNetwork.IsMasterClient;
        
        startButton.gameObject.SetActive(activateStartButton);
        text.SetActive(!enoughPlayer);
        roomName.text = "ID: "+PhotonNetwork.CurrentRoom.Name;
    }

    public void OnClickChangeColor(){
        ChoseColorPopup.ShowPopup();
    }

    

    


}
