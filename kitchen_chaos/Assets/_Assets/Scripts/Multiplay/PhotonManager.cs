using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Realtime;
using Photon.Pun;
using System;
using System.Linq;
using DG.DemiEditor;


public class CmdOrder
{
    public string receiver;
    public string functionName;
    public object[] data;

    public CmdOrder(string reciver, string functionName, params object[] data)
    {
        this.receiver = reciver;
        this.functionName = functionName;
        this.data = data;
    }
}

[System.Serializable]
public class RegionPing
{
    public string region;
    public int ping;
    public RegionPing(string region, int ping)
    {
        this.region = region;
        this.ping = ping;
    }

}

//using Game;
[RequireComponent(typeof(PhotonView))]
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager s;
    [SerializeField] bool _autoConnectToPhotonTest;
    [ShowIf(nameof(_autoConnectToPhotonTest))] public string defaultRoomName = "TestRoom2";

    public bool autoConnectToPhotonTest
    {
        get
        {
            if (!Application.isEditor)
                return false;

            return _autoConnectToPhotonTest;
        }
    }
    public bool isDoneInitServerList = false;

    public bool autoConncetToMaster = true;

    #region Event
    public Action onCreatedRoom;
    public Action onJoinRoom;
    public Action onJoinLobby;
    public Action onLeaveRoom;
    public Action onLeaveLobby;
    public Action onConnectToPhoton;
    public Action onConnectToServer;
    public Action<DisconnectCause> onDisconnect;
    public Action onSetMasterClient;
    public Action<string> onJoinRoomFailed;
    public Action<string> onJoinRandomRoomFailed;
    public Action<string> onCreateRoomFailed;
    public Action<Photon.Realtime.Player> onPlayerLeftRoom;
    public Action<Photon.Realtime.Player> onPlayerEnteredRoom;
    public bool isJoinedRoom => PhotonNetwork.InRoom;
    public bool isServerConnected => PhotonNetwork.IsConnected;
    public Photon.Realtime.Player myPlayerPhoton => PhotonNetwork.LocalPlayer;
    public Action<CmdOrder> onCallAnyCmdFunction;
    static RegionHandler prevRegion;
    public static List<RegionPing> allRegionPing = new List<RegionPing>();
    Action<List<RegionPing>> doneRefeshRegionPing;
    [ReadOnly]
    public List<Player> currentGamePlayers;
    string prevPing = null;

    public Player GetPlayerView(int viewId)
    {
        foreach (var player in currentGamePlayers)
        {
            if (player.photonView.ViewID == viewId)
                return player;
        }

        Debug.Log("Player not found: " + viewId);
        return null;

    }

    #endregion

    private void Awake()
    {
        if (s == null)
        {
            s = this;
        }
    }

    public PhotonView GetPhotonView(int id)
    {
        var views = PhotonNetwork.PhotonViewCollection;
        foreach (var view in views)
        {
            if (view.ViewID == id)
            {
                return view;
            }
        }
        Debug.LogError("PhotonView not found: " + id);
        return null;
    }

    public void DestroyPhotonView(int id)
    {
        var view = GetPhotonView(id);
        if (view == null)
            return;
        PhotonNetwork.Destroy(view);
    }

    public void DestroyPhotonView(PhotonView view)
    {
        PhotonNetwork.Destroy(view);
    }


    public void Init()
    {
        //set name to player
        if (UserData.currentUser != null)
            PhotonNetwork.NickName = UserData.currentUser.username;

        //PhotonNetwork.OfflineMode = autoConnectToPhoton;
        if (autoConnectToPhotonTest)
        {
            //Debug.Log("starting offline mode");
            // Set up Photon server settings
            //PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false; // Disable the default Photon Cloud server
            //PhotonNetwork.PhotonServerSettings.AppSettings.Server = "127.0.0.1"; // Set the IP address of the local Photon Server
            //PhotonNetwork.PhotonServerSettings.AppSettings.Port = 5058; // Set the port number of the local Photon Server



            ConnectToPhoton();
            onConnectToServer += () => { JoinLobby(); };
            onJoinLobby += () =>
            {
                JoinOrCreate(defaultRoomName);

            };
            //onJoinRoom += ()=>{ObjectEnum.MainPlayer.SpawnMultiplay(Vector3.zero, Quaternion.identity);};
            //CreateRoom();
        }
        else
        {
            //PhotonNetwork.PhotonServerSettings.AppSettings.Server = ""; // Set the IP address of the local Photon Server
            //PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0; // Set the port number of the local Photon Server
            if (autoConncetToMaster)
            {
                onLeaveRoom += () => { JoinLobby(); };
                ConnectToPhoton();
            }

        }
    }
    [Button]
    public void RefreshPing()
    {
        if (prevRegion == null)
        {
            Debug.Log("Region not found");
            return;
        }
        //Debug.Log("Refreshing ping");
        OnRegionListReceived(prevRegion);
    }
    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        //Debug.Log("Region list received");

        if (regionHandler == null)
            return;
        base.OnRegionListReceived(regionHandler);
        prevRegion = regionHandler;


        if (prevPing == null)
        {
            prevPing = regionHandler.SummaryToCache;
        }
        //Debug.Log("prevPing: " + prevPing);
        regionHandler.PingMinimumOfRegions((callback) =>
        {

            //Debug.Log("Ping minimum of regions");
            if (callback.EnabledRegions.Count == 0)
            {
                Debug.LogError("No region found");
                return;
            }

            allRegionPing.Clear();
            foreach (var i in callback.EnabledRegions)
            {
                allRegionPing.Add(new RegionPing(i.Code, i.Ping));
                //Debug.Log(i.Code + " " + i.Ping);
            }
            SortPing();
            //SortPing();
            doneRefeshRegionPing?.Invoke(allRegionPing);
            prevRegion = callback;

            if (UserSetting.regionSelected == "null")
            {
                UserSetting.regionSelected = allRegionPing[0].region;
            }
            //Debug.Log("Region selected: " + UserSetting.regionSelected);
            //Debug.Log(PhotonNetwork.NetworkClientState);

            if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToNameServer)
            {
                ConnectToPhoton(UserSetting.regionSelected);
            }

            isDoneInitServerList = true;

            //OnRegionListReceived(callback);
        }, prevPing);
        //prevRegion = regionHandler;
    }

    void SortPing()
    {
        allRegionPing = allRegionPing.OrderBy(x => x.ping).ToList();
    }

    public void EndSession()
    {
        PhotonNetwork.LeaveRoom();

        // Destroy(gameObject);
    }

    #region Photon Control
    public static void JoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }
    public static void JoinOrCreate(string name = "TestRoom")
    {
        PhotonNetwork.JoinOrCreateRoom(name, new RoomOptions(), TypedLobby.Default);
    }
    [Button]
    public static void CreateRoom(string name = "TestRoom")
    {
        PhotonNetwork.CreateRoom(name);
    }
    //log current state of photon
    [Button]
    public static void LogState()
    {
        Debug.Log("state: " + PhotonNetwork.NetworkClientState);

    }
    public static void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public static void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    [Button]
    public static void JoinLobby()
    {
        var p = PhotonNetwork.CountOfPlayersInRooms;
        PhotonNetwork.JoinLobby();
    }

    public static void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
    [Button]
    public static void ConnectToPhoton()
    {
        try
        {
            PhotonNetwork.ConnectUsingSettings();
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
    [Button]
    public void ConnectToPhoton(String region)
    {
        try
        {
            Debug.Log("Connect to region: " + region);
            PhotonNetwork.ConnectToRegion(region);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
    public static void ConnectToServer(string serverId)
    {
        try
        {
            PhotonNetwork.ConnectToRegion(serverId);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }


    public static void SetNickName(string name)
    {
        PhotonNetwork.NickName = name;
    }

    public static void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public static void SetMasterClient(Photon.Realtime.Player player)
    {
        PhotonNetwork.SetMasterClient(player);

    }

    public static GameObject Instantiate(ObjectEnum type, Vector3 position = default, Quaternion rotation = default)
    {
        var path = GameData.s.prefabPaths[type.ToString()];
        var g = PhotonNetwork.Instantiate(path, position, rotation);
        //Resources.Load()
        if (g == null)
            Debug.Log("Instantiate multiplay object failed" + type);
        return g;
    }
    #endregion

    public void CmdChangeState(int index)
    {
        photonView.RPC(nameof(RPCChangeState), RpcTarget.Others, index);
    }

    [PunRPC]
    public void RPCChangeState(int index)
    {
        //GameManager.s.SetState(index);
    }

    [Button]
    public void CmdCallFunction(CmdOrder cmdOrder)
    {
        photonView.RPC(nameof(RPCCallFunction), RpcTarget.All, cmdOrder.receiver, cmdOrder.functionName, cmdOrder.data);
    }

    [PunRPC]
    public void RPCCallFunction(string receiver, string functionName, object[] data)
    {
        //GameManager.s.CallFunction(functionName, data);
        //Debug.Log("Call function: "+ functionName + " on "+ receiver);
        var order = new CmdOrder(receiver, functionName, data);
        onCallAnyCmdFunction?.Invoke(order);
    }

    [Button]
    public void CmdEndGame()
    {
        photonView.RPC(nameof(RPCEndGame), RpcTarget.All);
    }

    [PunRPC]
    public void RPCEndGame()
    {
        Debug.Log("called rpc end game");
        TimeupPopup.ShowPopup().SetData(GameManager.getStageData, DeliveryManager.recipeDeliveredPoint);
    }


    public void OnSpawnInventory()
    {
        //photonView.RPC(nameof(RPCOnSpawnInventory), RpcTarget.Others);
    }

    public void RPCSpawnInventory()
    {
        //GameManager.s.SpawnInventory();
    }


    public void CmdAddPlate(int photonId)
    {
        int palteId = PhotonNetwork.AllocateViewID(false);
        photonView.RPC(nameof(RpcAddPlate), RpcTarget.All, photonId, palteId);
    }

    [PunRPC]
    public void RpcAddPlate(int photonId, int plateId)
    {
        var find = PhotonNetwork.GetPhotonView(photonId);
        if (find == null)
        {
            Debug.LogError("cant find id: " + photonId);
        }
        var kitchenObjet = find.GetComponent<KitchenObject>();
        kitchenObjet.AddPlateLocal(plateId);
    }

    public void CmdSpawnFoodObject(string objectType, int kitchenObjectSOId, int photonId, List<int> ingredient, bool isHavingPlate = false)
    {
        int viewID = PhotonNetwork.AllocateViewID(false);
        // Convert List<int> to int[] array for Photon serialization
        int[] ingredientArray = ingredient.ToArray();
        photonView.RPC(nameof(RpcSpawnKitchenObject), RpcTarget.All, objectType, kitchenObjectSOId, photonId, viewID, ingredientArray, isHavingPlate);
    }


    [PunRPC]
    public void RpcSpawnKitchenObject(string objectType, int kitchenObjectSOId, int parentPhotonId, int viewId, int[] ingredient, bool isHavingPlate)
    {
        Debug.Log("Ingredient count: " + ingredient.Length);
        foreach (var i in ingredient)
        {
            Debug.Log("Ingredient: " + i);
        }
        IKitchenContainable kitchenObjectParent = null;
        if (parentPhotonId != -1)
            kitchenObjectParent = PhotonNetwork.GetPhotonView(parentPhotonId).GetComponent<IKitchenContainable>();
        Transform kitchenObjectTransform = Instantiate(GameData.s.GetObject(objectType), Vector3.zero, Quaternion.identity).transform;
        kitchenObjectTransform.GetComponent<PhotonView>().ViewID = viewId;
        var ko = kitchenObjectTransform.GetComponent<KitchenObject>();

        ko.kitchenObjectSO = CookingBookSO.s.kitchenObjectSOs[kitchenObjectSOId];
        ko.CmdSetContainerParent(kitchenObjectParent);
        ko.AddIngredientIndexes(ingredient);
        if(isHavingPlate){
            ko.TryAddPlate();
        }
    }

    #region Callback
    public override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Connected to Photon");
        onConnectToPhoton?.Invoke();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Server");
        onConnectToServer?.Invoke();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Created Room");
        onCreatedRoom?.Invoke();
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        Debug.Log("Left Lobby");
        onLeaveLobby?.Invoke();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Left Room");
        onLeaveRoom?.Invoke();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log("Player Left Room" + otherPlayer.UserId);

        onPlayerLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("Player Entered Room" + newPlayer.UserId);
        onPlayerEnteredRoom?.Invoke(newPlayer);
    }


    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby");
        onJoinLobby?.Invoke();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Create Room Failed" + message);
        onCreateRoomFailed?.Invoke(message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected" + cause);

        onDisconnect?.Invoke(cause);
        //reconnect
        if (UserSetting.regionSelected != "null")
        {
            Debug.Log("Reconnect to region: " + UserSetting.regionSelected);
            ConnectToPhoton(UserSetting.regionSelected);
        }
    }


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room");
        onJoinRoom?.Invoke();
    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Join Room Failed" + message);
        onJoinRoomFailed?.Invoke(message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Join Random Room Failed" + message);
        onJoinRandomRoomFailed?.Invoke(message);
    }





    #endregion

}
