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
using System;

public class CreateRoomPopup : BasePopup<CreateRoomPopup>
{
    #region Constants
    const string CMD_SWITCH_STAGE = "CmdSwitchStage";
    const string CMD_NEXT_SCENE = "CmdNextScene";
    const string CMD_AI_SCENE = "CmdAiScene";
    #endregion

    #region Variables and Components
    public TextMeshProUGUI roomName;
    public List<PlayerUIElement> playerUIElements;
    public GameObject startButton;
    public PhotonView photonView;
    public StageUI stageUIPref;
    public Transform stageContentParent;
    [HideInInspector] public List<StageUI> stageUIs;
    public Image previewImage;
    public GameObject previewImageParent;
    public GameObject text;
    [SerializeField] private StarController currentLevelStarController;
    [SerializeField] private Button copyIdButton;
    [HideInInspector] public StageData selectStage = null;

    public GameObject stageParent;
    int currentSceneId = 0;
    public Image changeSkinColorButtonImage;
    private int _roomPlayer = 2;
    #endregion

    #region Editors
    [Button("Move to GameScene")]
    public void OnStartButtonClick()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;
        var order = new CmdOrder(nameof(CreateRoomPopup), CMD_NEXT_SCENE);
        PhotonManager.s.CmdCallFunction(order);
    }

    [Button("Move to AI Test")]
    public void MoveToAiTest()
    {
        //PhotonNetwork.AutomaticallySyncScene = true;
        var order = new CmdOrder(nameof(CreateRoomPopup), CMD_AI_SCENE);
        PhotonManager.s.CmdCallFunction(order);
    }
    #endregion

    #region Unity functions
    public void OnExitButtonClick()
    {
        PhotonNetwork.LeaveRoom();
    }
    void Start()
    {
        currentSceneId = 0;
        SetupStages();
        stageParent.SetActive(PhotonNetwork.IsMasterClient);
        if (SectionData.s.isSinglePlay)
        {
            _roomPlayer = 1;
        }
        else
        {
            _roomPlayer = 2;
        }
        RefreshUI();
        copyIdButton.onClick.AddListener(CopyId);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        PhotonManager.s.onJoinRoom += RefreshUI;
        PhotonManager.s.onLeaveRoom += OnLeaveRoom;
        PhotonManager.s.onPlayerEnteredRoom += OnPlayerEnterRoom;
        PhotonManager.s.onPlayerLeftRoom += OnPlayerLeaveRoom;
        PhotonManager.s.onCallAnyCmdFunction += OnCallAnyCmdFunction;
    }
    protected override void OnDisable()
    {
        base.OnDisable();

        PhotonManager.s.onJoinRoom -= RefreshUI;
        PhotonManager.s.onLeaveRoom -= OnLeaveRoom;
        PhotonManager.s.onPlayerEnteredRoom -= OnPlayerEnterRoom;
        PhotonManager.s.onPlayerLeftRoom -= OnPlayerLeaveRoom;
        PhotonManager.s.onCallAnyCmdFunction -= OnCallAnyCmdFunction;

    }
    #endregion


    void OnCallAnyCmdFunction(CmdOrder order)
    {
        if (order.receiver != nameof(CreateRoomPopup)) return;

        if (order.functionName == CMD_SWITCH_STAGE)
        {
            RpcSwitchStage((int)order.data[0], (int)order.data[1]);
        }
        if (order.functionName == CMD_NEXT_SCENE)
        {
            GameManager.levelId = currentSceneId;
            PhotonNetwork.LoadLevel(GameData.s.GetStage(currentSceneId).sceneName);
        }
        if (order.functionName == CMD_AI_SCENE)
        {
            GameManager.levelId = currentSceneId;
            PhotonNetwork.LoadLevel("AI_Test");
        }
    }

    private void CopyId()
    {
        GUIUtility.systemCopyBuffer = PhotonNetwork.CurrentRoom.Name;
    }


    void OnPlayerEnterRoom(Photon.Realtime.Player player)
    {
        RefreshUI();


        if (PhotonNetwork.IsMasterClient)
        {
            CmdSwitchStage(selectStage.levelId, selectStage.star);
        }

    }

    void OnPlayerLeaveRoom(Photon.Realtime.Player player)
    {
        RefreshUI();
    }


    void OnLeaveRoom()
    {
        HidePopup();
    }


    async void SetupStages()
    {
        var stages = GameData.s.stages;

        stageUIs.Add(stageUIPref);
        while (stageUIs.Count != stages.Count)
        {
            if (stageUIs.Count < stages.Count)
            {
                var stageUI = Instantiate(stageUIPref, stageContentParent.transform);
                stageUIs.Add(stageUI);
            }
            else
            {
                var stageUI = stageUIs[stageUIs.Count - 1];
                stageUIs.Remove(stageUI);
                Destroy(stageUI.gameObject);
            }
        }

        // Force reload stage data from PlayerPrefs
        for (int i = 0; i < stages.Count; i++)
        {
            var stageData = stages[i];
            Debug.Log("Setup stage " + stageData.levelId + " with star: " + stageData.star);
            stageUIs[i].SetData(stageData, OnSwitchStage);
        }

        foreach (var i in stageUIs)
        {
            i.gameObject.SetActive(true);
            i.transform.DOScale(Vector3.zero, 0.25f).From().SetEase(Ease.OutBack);
            await Task.Delay(100);
        }

        // Start with the first stage
        if (stages.Count > 0)
        {
            CmdSwitchStage(stages[0].levelId, stages[0].star);
        }
    }

    void OnSwitchStage(StageData stageData)
    {
        CmdSwitchStage(stageData.levelId, stageData.star);
    }

    void CmdSwitchStage(int id, int star)
    {
        Debug.Log("CmdSwitchStage " + id);
        var order = new CmdOrder(nameof(CreateRoomPopup), CMD_SWITCH_STAGE, id, star);
        PhotonManager.s.CmdCallFunction(order);
    }

    void RpcSwitchStage(int id, int star)
    {
        Debug.Log("RpcSwitchStage " + id);
        currentSceneId = id;
        var stageData = GameData.s.GetStage(id);
        
        // Make sure we have the latest score from PlayerPrefs
        int savedStar = PlayerPrefs.GetInt("level." + id, 0);
        if (savedStar > star) {
            star = savedStar; // Use the higher value
        }
        
        foreach (var stage in stageUIs)
        {
            if (stage.stageData.levelId == stageData.levelId)
                stage.SetSelect();
            else
                stage.Unselect();
        }

        currentLevelStarController.ShowPoint(stageData.pointTarget);
        currentLevelStarController.ShowStar(star);
        previewImage.sprite = stageData.previewImage;
        selectStage = stageData;

        var rootScale = 2.225f;

        previewImageParent.transform.DOScale(rootScale, 0.25f).From(0).SetEase(Ease.OutBack);
    }

    public void RefreshUI()
    {
        changeSkinColorButtonImage.color = UserSetting.colorSkin.color;
        foreach (var i in playerUIElements)
        {
            i.gameObject.SetActive(false);
        }

        var roomPlayer = PhotonNetwork.PlayerList.Length;

        for (int i = 0; i < roomPlayer; i++)
        {
            playerUIElements[i].gameObject.SetActive(true);
            //playerUIElements[i].transform.localScale = Vector3.zero;
            playerUIElements[i].transform.DOScale(1f, 0.25f).From(0).SetEase(Ease.OutBack);

            playerUIElements[i].SetData(PhotonNetwork.PlayerList[i]);
        }

        var enoughPlayer = roomPlayer == _roomPlayer;
        var activateStartButton = enoughPlayer && PhotonNetwork.IsMasterClient;

        startButton.gameObject.SetActive(activateStartButton);
        text.SetActive(!enoughPlayer);
        roomName.text = "ID: " + PhotonNetwork.CurrentRoom.Name;
    }

    public void OnClickChangeColor()
    {
        ChoseColorPopup.ShowPopup();
    }
}
