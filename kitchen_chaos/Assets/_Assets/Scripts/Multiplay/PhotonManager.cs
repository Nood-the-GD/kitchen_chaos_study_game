using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Realtime;
using Photon.Pun;
using System;

public class CmdOrder{
    public string receiver;
    public string functionName;
    public object[] data;

    public CmdOrder(string reciver, string functionName, params object[] data){
        this.receiver = reciver;
        this.functionName = functionName;
        this.data = data;
    }
}

//using Game;
[RequireComponent(typeof(PhotonView))]
public class PhotonManager : MonoBehaviourPunCallbacks
{
    public static PhotonManager s;
    [SerializeField]bool _autoConnectToPhotonTest;
    [ShowIf(nameof(_autoConnectToPhotonTest))]public string defaultRoomName = "TestRoom2";

    public bool autoConnectToPhotonTest{
        get{
            if(!Application.isEditor)
                return false;

            return _autoConnectToPhotonTest;
        }
    }

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
    public  Action<string> onJoinRoomFailed;
    public Action<string> onJoinRandomRoomFailed;
    public Action<string> onCreateRoomFailed;
    public Action<Photon.Realtime.Player> onPlayerLeftRoom;
    public Action<Photon.Realtime.Player> onPlayerEnteredRoom;
    public bool isJoinedRoom => PhotonNetwork.InRoom;
    public Photon.Realtime.Player myPlayerPhoton => PhotonNetwork.LocalPlayer;

    public Action<CmdOrder> onCallAnyCmdFunction;
    

    [ReadOnly]
    public List<Player> currentGamePlayers;

    public Player GetPlayerView(int viewId){
        foreach(var player in currentGamePlayers){
            if(player.photonView.ViewID == viewId)
                return player;
        }
        
        Debug.Log("Player not found: "+ viewId);
        return null;

    }

#endregion

    private void Awake(){
        if(s == null){
            s = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
            return;
        }
    }

    public PhotonView GetPhotonView(int id){
        var views = PhotonNetwork.PhotonViewCollection;
        foreach(var view in views){
            if(view.ViewID == id){
                return view;
            }
        }
        Debug.LogError("PhotonView not found: "+ id);
        return null;
    }

    public void DestroyPhotonView(int id){
        var view = GetPhotonView(id);
        if(view == null)
            return;
        PhotonNetwork.Destroy(view);
    }

    public void DestroyPhotonView(PhotonView view){
        PhotonNetwork.Destroy(view);
    }


    void Start(){
        Init();
    }

    void Init(){
        //set name to player
        if(!UserData.isInitName){
            PhotonNetwork.NickName = "User " + UnityEngine.Random.Range(0, 1000);
        }
        else{
            PhotonNetwork.NickName = UserData.userName;
        }

        //PhotonNetwork.OfflineMode = autoConnectToPhoton;
        if(autoConnectToPhotonTest){
            //Debug.Log("starting offline mode");
             // Set up Photon server settings
            //PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = false; // Disable the default Photon Cloud server
            //PhotonNetwork.PhotonServerSettings.AppSettings.Server = "127.0.0.1"; // Set the IP address of the local Photon Server
            //PhotonNetwork.PhotonServerSettings.AppSettings.Port = 5058; // Set the port number of the local Photon Server


            
            ConnectToPhoton();
            onConnectToServer += ()=>{JoinLobby();};
            onJoinLobby += ()=>{
                JoinOrCreate(defaultRoomName);
                
            };
            //onJoinRoom += ()=>{ObjectEnum.MainPlayer.SpawnMultiplay(Vector3.zero, Quaternion.identity);};
            //CreateRoom();
        }
        else{
            //PhotonNetwork.PhotonServerSettings.AppSettings.Server = ""; // Set the IP address of the local Photon Server
            //PhotonNetwork.PhotonServerSettings.AppSettings.Port = 0; // Set the port number of the local Photon Server
            if(autoConncetToMaster){
                ConnectToPhoton();
            }

        }
    }
    
#region Photon Controll
    public static void JoinRoom(string name)
    {
        PhotonNetwork.JoinRoom(name);
    }
    public static void JoinOrCreate(string name = "TestRoom"){
        PhotonNetwork.JoinOrCreateRoom(name, new RoomOptions(), TypedLobby.Default);
    }
    [Button]
    public static void CreateRoom(string name = "TestRoom")
    {
        PhotonNetwork.CreateRoom(name);
    }
    //log current state of photon
    [Button]
    public static void LogState(){
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
         var p =PhotonNetwork.CountOfPlayersInRooms;
        PhotonNetwork.JoinLobby();
    }

    public static void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
    }
    [Button]
    public static void ConnectToPhoton(){
        try{
            PhotonNetwork.ConnectUsingSettings();
        }catch(System.Exception e){
            Debug.Log(e);
        }
    }

    public static void ConnectToServer(string serverId){
        try{
            PhotonNetwork.ConnectToRegion(serverId);
        }catch(System.Exception e){
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

    public static GameObject Instantiate(ObjectEnum type, Vector3 position = default, Quaternion rotation= default)
    {
        var path = GameData.s.prefabPaths[type.ToString()];
        var g = PhotonNetwork.Instantiate(path, position, rotation);
        //Resources.Load()
        if(g==null)
            Debug.Log("Instantiate multiplay object failed"+ type);
        return g;
    }
#endregion

    public void CmdChangeState(int index){
        photonView.RPC(nameof(RPCChangeState), RpcTarget.Others, index);
    }

    [PunRPC]
    public void RPCChangeState(int index){
        //GameManager.s.SetState(index);
    }

    [Button]
    public void CmdCallFunction(CmdOrder cmdOrder){
        photonView.RPC(nameof(RPCCallFunction), RpcTarget.All, cmdOrder.receiver,cmdOrder.functionName, cmdOrder.data);
    }

    [PunRPC]
    public void RPCCallFunction(string reciver,string functionName, object[] data){
        //GameManager.s.CallFunction(functionName, data);
        //Debug.Log("Call function: "+ functionName + " on "+ reciver);
        var order = new CmdOrder(reciver, functionName, data);
        onCallAnyCmdFunction?.Invoke(order);
    }
    

    public void CmdEndGame(){
        photonView.RPC(nameof(RPCEndGame), RpcTarget.All);
    } 

    [PunRPC]
    public void RPCEndGame(){
        TimeupPopup.ShowPopup().SetData(GameManager.getStageData ,DeliveryManager.recipeDeliveredPoint);
    }


    public void OnSpawnInventory(){
        //photonView.RPC(nameof(RPCOnSpawnInventory), RpcTarget.Others);
    }

    public void RPCSpawnInventory(){
        //GameManager.s.SpawnInventory();
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
        Debug.Log("Player Left Room"+ otherPlayer.UserId);
    
        onPlayerLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("Player Entered Room"+ newPlayer.UserId);
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
        Debug.Log("Create Room Failed"+ message);
        onCreateRoomFailed?.Invoke(message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Disconnected"+ cause);
        onDisconnect?.Invoke(cause);
        
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
        Debug.Log("Join Room Failed"+ message);
        onJoinRoomFailed?.Invoke(message);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Join Random Room Failed"+ message);
        onJoinRandomRoomFailed?.Invoke(message);
    }
    




#endregion

}
