using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections;
using Cysharp.Threading.Tasks;

public static class ServerConnect
{
    // ================================
    // Public events for callbacks
    // ================================
    public static event Action OnConnected;
    public static event Action OnDisconnected;
    public static event Action<string> OnMessageReceived;
    public static event Action<byte[]> OnDrawingImageReceived;
    public static event Action OnMyUserDataUpdate;
    public static event Action OnSocialDataUpdate;
    public static Action<MessageData> OnChatMessage;
    public static event Action<string> OnDrawingRaw;
    // (Add additional events as needed for chat messages, picture data, etc.)

    // ================================
    // Private connection fields
    // ================================
    private static WebSocket _websocket;
    private static bool _isConnected = false;
    private static bool _isConnecting = false;
    private static bool _isReconnecting = false;
    private static int _reconnectAttempt = 0;
    private const int _maxReconnectAttempts = 5;
    private static readonly TimeSpan _initialReconnectDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _maxReconnectDelay = TimeSpan.FromSeconds(30);

    // ================================
    // Ping timer fields
    // ================================
    private static Timer _pingTimer;
    private static readonly TimeSpan _pingInterval = TimeSpan.FromSeconds(30);
    private static readonly string _pingMessage = JsonConvert.SerializeObject(new
    {
        action = "message",
        data = new
        {
            function = "ping",
            @params = new { }
        }
    });
    private static DateTime? _lastPingTime = null;

    // ================================
    // Public settings
    // ================================
    // Set this property externally after login (for example, using Firebase or your own auth).
    public static bool NotiMessage = true;
    public static bool AutoReconnect = true;

    // ================================
    // Connect to the server
    // ================================
    public static async Task ConnectServer()
    {
        if (_isConnecting)
            return;

        _isConnecting = true;
        Debug.Log("<color=green>Connecting to server...</color>");

        if (string.IsNullOrEmpty(UserData.currentUser.uid))
        {
            Debug.LogError("No user logged in!");
            _isConnecting = false;
            return;
        }

        string uid = UserData.currentUser.uid;
        Debug.Log("<color=green>Connect for uid: " + uid + "</color>");
        string urlString = $"wss://4vup7tn95f.execute-api.ap-southeast-1.amazonaws.com/production/?uid={uid}";

        try
        {
            _websocket = new WebSocket(urlString);

            // Subscribe to WebSocket events.
            _websocket.OnOpen += (sender, e) =>
            {
                Debug.Log("<color=green>Connected to server.</color>");
                _isConnected = true;
                _reconnectAttempt = 0;
                _isReconnecting = false;
                UniTask.Post(() => OnConnected?.Invoke());
                StartPingTimer();
            };

            _websocket.OnMessage += (sender, e) =>
            {
                string msg = e.Data;
                UniTask.Post(() => OnMessageReceived?.Invoke(msg));
                ServerDataHandler(msg).Forget();
            };

            _websocket.OnClose += async (sender, e) =>
            {
                Debug.Log("<color=green>WebSocket connection closed.</color>");
                StopPingTimer();
                _isConnected = false;
                UniTask.Post(() => OnDisconnected?.Invoke());

                // if (!_isReconnecting)
                // {
                //     await AttemptReconnect();
                // }
                // else
                // {
                //     Debug.Log("<color=green>Already reconnecting. Skipping additional reconnect attempt.</color>");
                // }
            };

            _websocket.OnError += async (sender, e) =>
            {
                Debug.Log("<color=green>WebSocket error: " + e.Message + "</color>");
                StopPingTimer();
                _isConnected = false;
                UniTask.Post(() => OnDisconnected?.Invoke());

                if (!_isReconnecting)
                {
                    await AttemptReconnect();
                }
                else
                {
                    Debug.Log("<color=green>Already reconnecting. Skipping additional reconnect attempt.</color>");
                }
            };

            // Initiate the connection
            _websocket.Connect();
            _isConnecting = false;
        }
        catch (Exception ex)
        {
            Debug.LogError("Connection failed: " + ex.Message);
            _isConnecting = false;
            _isConnected = false;
            await AttemptReconnect();
        }
    }

    // ================================
    // Attempt reconnect with exponential backoff
    // ================================
    private static async Task AttemptReconnect()
    {
        if (!AutoReconnect)
            return;

        if (_isReconnecting)
        {
            Debug.Log("Already attempting to reconnect. Ignoring additional attempts.");
            return;
        }

        _isReconnecting = true;

        if (_reconnectAttempt >= _maxReconnectAttempts)
        {
            Debug.LogError("Max reconnection attempts reached. Giving up.");
            _isReconnecting = false;
            return;
        }

        _reconnectAttempt++;
        // Calculate delay with exponential backoff
        int delaySeconds = (int)(_initialReconnectDelay.TotalSeconds * Math.Pow(2, _reconnectAttempt - 1));
        if (delaySeconds > _maxReconnectDelay.TotalSeconds)
            delaySeconds = (int)_maxReconnectDelay.TotalSeconds;

        Debug.Log($"Reconnecting in {delaySeconds} seconds... (Attempt {_reconnectAttempt})");
        await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        _isConnecting = false;
        await ConnectServer();
    }

    // ================================
    // Disconnect from the server
    // ================================
    public static void Disconnect()
    {
        if (_websocket != null)
        {
            _websocket.Close();
            _websocket = null;
        }

        StopPingTimer();
        Debug.Log("Disconnected from server.");
        _isConnected = false;
        UniTask.Post(() => OnDisconnected?.Invoke());

        // Optionally reset reconnection parameters.
        _isReconnecting = false;
        _reconnectAttempt = 0;
    }

    // ================================
    // Send a message through the WebSocket
    // ================================
    public static void SendMessage(string message)
    {
        if (_websocket != null && _isConnected)
        {
            _websocket.Send(message);
            Debug.Log("Sent message: " + message);
        }
        else
        {
            Debug.LogWarning("Cannot send message. No active WebSocket connection.");
        }
    }

    // ================================
    // Start and stop the ping timer
    // ================================
    private static void StartPingTimer()
    {
        StopPingTimer(); // Ensure any previous timer is stopped.
        _pingTimer = new Timer((state) => {
            SendPing();
        }, null, _pingInterval, _pingInterval);
        Debug.Log("Ping timer started with interval " + _pingInterval);
    }

    private static void StopPingTimer()
    {
        if (_pingTimer != null)
        {
            _pingTimer.Dispose();
            _pingTimer = null;
            Debug.Log("Ping timer stopped.");
        }
    }

    // ================================
    // Send a ping message
    // ================================
    private static void SendPing()
    {
        _lastPingTime = DateTime.Now;
        SendMessage(_pingMessage);
        Debug.Log("Ping sent to server at " + _lastPingTime);
    }

    // ================================
    // Check connection state
    // ================================
    public static bool IsConnected()
    {
        return _isConnected;
    }

    // ================================
    // Dispose resources and unsubscribe events
    // ================================
    public static void Dispose()
    {
        if (_websocket != null)
        {
            _websocket.Close();
            _websocket = null;
        }

        StopPingTimer();

        // Remove all event subscriptions.
        OnConnected = null;
        OnDisconnected = null;
        OnMessageReceived = null;
        OnDrawingImageReceived = null;
        OnMyUserDataUpdate = null;
        OnSocialDataUpdate = null;
        OnDrawingRaw = null;

        _isReconnecting = false;
        _reconnectAttempt = 0;
        _isConnected = false;

        Debug.Log("ServerConnect disposed. All resources cleaned up.");
    }

    // ================================
    // Handle incoming server data
    // ================================
    private static async UniTaskVoid ServerDataHandler(string message)
    {
        try
        {
            // Deserialize the JSON message into a dictionary.
            var body = JToken.Parse(message);
            Debug.Log("ServerDataHandler: " + body);
            
            // Try to get the messageType value, returns null if key doesn't exist
            var messageType = body?["messageType"];
            if (body == null || messageType == null)
            {
                Debug.LogError("Invalid message format.");
                return;
            }

            string messageTypeStr = messageType.ToString();
            JToken data = null;
            if (body["data"] != null)
            {
                data = body["data"];
            }
            else{
                Debug.Log("No data found in message.");
                return;
            }

            if (messageTypeStr == "pong")
            {
                if (_lastPingTime.HasValue)
                {
                    var pongTime = DateTime.Now;
                    var delay = (pongTime - _lastPingTime.Value).TotalMilliseconds;
                    Debug.Log("Ping-Pong delay: " + delay + " ms");
                    _lastPingTime = null;
                }
                else
                {
                    Debug.Log("Received pong without a corresponding ping.");
                }
                return;
            }


            if (messageTypeStr == "updateWaitingRoomMessage")
            {
                // Example: extract a "content" field from the data (assumes data is a JSON object).
                var dataObj = data as JObject;
                if (dataObj != null)
                {
                    Debug.Log("Message content: " + dataObj["content"]);
                    // TODO: Update your waiting room messages and invoke related events.
                }
            }
            else if (messageTypeStr == "updateSocial")
            {
                var p = await LambdaAPI.GetMySocial();
                var socialData = p.jToken.ToObject<SocialData>();
                
                await UniTask.SwitchToMainThread();
                SocialData.UpdateMySocial(socialData);
                OnSocialDataUpdate?.Invoke();
            }
            else if (messageTypeStr == "updateChatMessage")
            {
                if(data["message"] == null){
                    Debug.LogError("No message found in data.");
                    return;
                }
                
                // Create local copies of the data we need
                var messageDataCopy = data["message"].ToObject<MessageData>();   
                var conversationIdCopy = data["conversationId"].ToString();
                
                // Switch to main thread for Unity operations
                await UniTask.SwitchToMainThread();
                
                try {
                    var messageChannel = new MessageDataChannel(conversationIdCopy, messageDataCopy);
                    SocialData.UpdateChatSummary(conversationIdCopy, messageDataCopy.content);
                    ConversationData.AddNewMessage(conversationIdCopy, messageDataCopy);
                    
                    Debug.Log("updateChatMessage processed on main thread.");
                    OnChatMessage?.Invoke(messageDataCopy);
                }
                catch (Exception ex) {
                    Debug.LogError("Error processing chat message on main thread: " + ex.Message);
                }
            }
            else if (messageTypeStr == "createChatMessage")
            {
                Debug.Log("createChatMessage received.");
                var messageDataCopy = data["message"].ToObject<MessageData>();   
                var conversationIdCopy = data["conversationId"].ToString();
                var usersCopy = data["users"];
                
                // Switch to main thread for Unity operations
                await UniTask.SwitchToMainThread();
                
                try {
                    var messageChannel = new MessageDataChannel(conversationIdCopy, messageDataCopy);

                    SocialData.UpdateChatSummary(conversationIdCopy, messageDataCopy.content);
                    var otherUid = usersCopy[0];
                    if(otherUid.ToString() != UserData.mineUid){
                        otherUid = usersCopy[1];
                    }

                    SocialData.AddChatSummary(conversationIdCopy, messageDataCopy.content, otherUid.ToString());
                    ConversationData.AddNewConvo(conversationIdCopy, messageDataCopy, otherUid.ToString());
                    if(messageDataCopy.userId != UserData.mineUid){
                        //show noti here
                    }
 
                    OnChatMessage.Invoke(messageDataCopy);
                }
                catch (Exception ex) {
                    Debug.LogError("Error processing chat message on main thread: " + ex.Message);
                }
            }
            else if (messageTypeStr == "inAppNoti")
            {
                var dataObj = data as JObject;
                if (dataObj != null)
                {
                    string notiType = dataObj["notiType"]?.ToString();
                    if (notiType == "friendRequest")
                    {
                        string fromUserName = dataObj["fromUserName"]?.ToString();
                        string fromUid = dataObj["otherUid"]?.ToString();
                        string notification = $"{fromUserName} đã gửi lời mời kết bạn";
                        Debug.Log(notification);
                        
                        await UniTask.SwitchToMainThread();
                        // TODO: Update your friend request data and notify the UI.
                        OnSocialDataUpdate?.Invoke();
                    }
                    else if (notiType == "acceptFriendRequest")
                    {
                        string fromUserName = dataObj["fromUserName"]?.ToString();
                        string fromUid = dataObj["otherUid"]?.ToString();
                        string notification = $"Bạn và {fromUserName} đã trở thành bạn";
                        Debug.Log(notification);
                        
                        await UniTask.SwitchToMainThread();
                        SocialData.AddFriend(fromUid);

                        // TODO: Update your friend list and notify the UI.
                        OnSocialDataUpdate?.Invoke();
                    }
                }
            }
            else if (messageTypeStr == "updateWaitingRoom")
            {
                Debug.Log("updateWaitingRoom received.");
                // TODO: Update your waiting room data.
            }
            else
            {
                Debug.LogError("Undefined message type: " + messageTypeStr);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error handling server message: " + ex.Message);
            Debug.LogError("Stack trace: " + ex.StackTrace);
        }
    }
}
