using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
    public static string CurrentUserUid;  
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
        Debug.Log("Connecting to server...");

        if (string.IsNullOrEmpty(CurrentUserUid))
        {
            Debug.LogError("No user logged in!");
            _isConnecting = false;
            return;
        }

        string uid = CurrentUserUid;
        Debug.Log("Connect for uid: " + uid);
        string urlString = $"wss://4vup7tn95f.execute-api.ap-southeast-1.amazonaws.com/production/?uid={uid}";

        try
        {
            _websocket = new WebSocket(urlString);

            // Subscribe to WebSocket events.
            _websocket.OnOpen += (sender, e) =>
            {
                Debug.Log("Connected to server.");
                _isConnected = true;
                _reconnectAttempt = 0;
                _isReconnecting = false;
                OnConnected?.Invoke();
                StartPingTimer();
            };

            _websocket.OnMessage += (sender, e) =>
            {
                string msg = e.Data;
                OnMessageReceived?.Invoke(msg);
                ServerDataHandler(msg);
            };

            _websocket.OnClose += async (sender, e) =>
            {
                Debug.Log("WebSocket connection closed.");
                StopPingTimer();
                _isConnected = false;
                OnDisconnected?.Invoke();

                if (!_isReconnecting)
                {
                    await AttemptReconnect();
                }
                else
                {
                    Debug.Log("Already reconnecting. Skipping additional reconnect attempt.");
                }
            };

            _websocket.OnError += async (sender, e) =>
            {
                Debug.Log("WebSocket error: " + e.Message);
                StopPingTimer();
                _isConnected = false;
                OnDisconnected?.Invoke();

                if (!_isReconnecting)
                {
                    await AttemptReconnect();
                }
                else
                {
                    Debug.Log("Already reconnecting. Skipping additional reconnect attempt.");
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
        OnDisconnected?.Invoke();

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
    private static void ServerDataHandler(string message)
    {
        try
        {
            // Deserialize the JSON message into a dictionary.
            var body = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            if (body == null || !body.ContainsKey("messageType"))
            {
                Debug.LogError("Invalid message format.");
                return;
            }

            string messageType = body["messageType"].ToString();
            object data = null;
            if (body.ContainsKey("data"))
            {
                data = body["data"];
            }

            if (messageType == "pong")
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

            Debug.Log("Received streaming message: " + message);

            if (messageType == "updateWaitingRoomMessage")
            {
                // Example: extract a “content” field from the data (assumes data is a JSON object).
                var dataObj = data as JObject;
                if (dataObj != null)
                {
                    Debug.Log("Message content: " + dataObj["content"]);
                    // TODO: Update your waiting room messages and invoke related events.
                }
            }
            else if (messageType == "updateSocial")
            {
                // TODO: Implement your logic to update social data (for example, call a helper method)
                OnSocialDataUpdate?.Invoke();
            }
            else if (messageType == "updateChatMessage")
            {
                // TODO: Parse your chat message and update your data structures.
                Debug.Log("updateChatMessage received.");
                // (You might trigger an OnChatMessage event here.)
            }
            else if (messageType == "createChatMessage")
            {
                Debug.Log("createChatMessage received.");
                // TODO: Handle creation of a new chat message.
            }
            else if (messageType == "inAppNoti")
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
                        // TODO: Update your friend request data and notify the UI.
                        OnSocialDataUpdate?.Invoke();
                    }
                    else if (notiType == "acceptFriendRequest")
                    {
                        string fromUserName = dataObj["fromUserName"]?.ToString();
                        string fromUid = dataObj["otherUid"]?.ToString();
                        string notification = $"Bạn và {fromUserName} đã trở thành bạn";
                        Debug.Log(notification);
                        // TODO: Update your friend list and notify the UI.
                        OnSocialDataUpdate?.Invoke();
                    }
                }
            }
            else if (messageType == "updateWaitingRoom")
            {
                Debug.Log("updateWaitingRoom received.");
                // TODO: Update your waiting room data.
            }
            else if (messageType == "updateDrawingImage")
            {
                // The data is assumed to be a base64-encoded string.
                string base64String = data.ToString();
                byte[] imageBytes = Convert.FromBase64String(base64String);
                OnDrawingImageReceived?.Invoke(imageBytes);
            }
            else if (messageType == "updateDrawingRaw")
            {
                string rawData = data.ToString();
                OnDrawingRaw?.Invoke(rawData);
            }
            else if (messageType == "updateFindObjectImage")
            {
                Debug.Log("updateFindObjectImage received.");
                // TODO: Convert the data to your PictureData object and trigger an event.
            }
            else
            {
                Debug.LogError("Undefined message type: " + messageType);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error handling server message: " + ex.Message);
        }
    }
}
