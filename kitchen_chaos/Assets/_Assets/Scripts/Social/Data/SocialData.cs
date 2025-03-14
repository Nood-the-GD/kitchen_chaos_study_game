using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.Serialization;
using UnityEngine;

#region Existing Message Classes

[Serializable]
public class MessageData
{
    public string content;
    public long timestamp;
    public string userId;

    // Constructor
    public MessageData(string content, long timestamp, string userId)
    {
        this.content = content;
        this.timestamp = timestamp;
        this.userId = userId;
    }

    /// <summary>
    /// Creates an instance from a JSON string.
    /// Uses Unity's JsonUtility.
    /// </summary>
    public static MessageData FromJson(string json)
    {
        return JsonUtility.FromJson<MessageData>(json);
    }

    /// <summary>
    /// Creates an instance from a JSON string but overrides the timestamp.
    /// </summary>
    public static MessageData FromJson2(int timestamp, string json)
    {
        MessageData data = JsonUtility.FromJson<MessageData>(json);
        data.timestamp = timestamp;
        return data;
    }

    /// <summary>
    /// Converts the instance to a JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    /// <summary>
    /// Creates a copy of the instance with optional modifications.
    /// </summary>
    public MessageData CopyWith(string content = null, int? timestamp = null, string userId = null)
    {
        return new MessageData(
            content ?? this.content,
            timestamp ?? this.timestamp,
            userId ?? this.userId
        );
    }

    public override string ToString()
    {
        return string.Format("MessageData(content: {0}, timestamp: {1}, userId: {2})", content, timestamp, userId);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        MessageData other = obj as MessageData;
        if (other == null)
            return false;

        return this.content == other.content &&
               this.timestamp == other.timestamp &&
               this.userId == other.userId;
    }

    public override int GetHashCode()
    {
        return content.GetHashCode() ^ timestamp.GetHashCode() ^ userId.GetHashCode();
    }
}


#endregion

#region Social Data Classes (Converted from Dart)

// Represents a friend request or similar object.
[Serializable]
public class OtherRequest
{
    public string uid;
    public int timestamp;


    /// <summary>
    /// Converts this instance to a JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

// Represents a summary of a chat with another user.
[Serializable]
public class ChatSummary
{
    public string id;
    public string message;
    public string otherUid;

    // Constructor
    public ChatSummary(string id, string content, string otherUid)
    {
        this.id = id;
        this.message = content;
        this.otherUid = otherUid;
    }


    /// <summary>
    /// Converts this instance to a JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

// Main container for social-related data.
// Note: In Dart the code uses a reactive type (Rxn) for mySocialData;
// here we simply use a static instance. If you require event notifications
// in Unity, consider implementing C# events or UnityEvents.
[Serializable]
public class SocialData
{
    // Other fields remain unchanged.
    public List<string> friends;
    // Changed type from Dictionary<string, OtherRequest> to Dictionary<string, int>
    public Dictionary<string, long> otherRequest; //uid, time utc
    public List<string> myRequest;
    public Dictionary<string, ChatSummary> chatSummary;

    // Static instance similar to Rxn<SocialData> in Dart.
    public static SocialData mySocialData;
    
    // Event that will be triggered when a friend is added
    public delegate void FriendAddedEventHandler(string friendUid);
    public static event FriendAddedEventHandler OnFriendAdded;

    // Constructor with default empty collections if none provided.
    public SocialData(
        List<string> friends = null,
        Dictionary<string, long> otherRequest = null,
        List<string> myRequest = null,
        Dictionary<string, ChatSummary> chatSummary = null)
    {
        this.friends = friends ?? new List<string>();
        this.otherRequest = otherRequest ?? new Dictionary<string, long>();
        this.myRequest = myRequest ?? new List<string>();
        this.chatSummary = chatSummary ?? new Dictionary<string, ChatSummary>();
    }


    public void SetUp(){
        foreach(var chat in chatSummary){
            chat.Value.id = chat.Key;
        }
    }

    /// <summary>
    /// Retrieves the ChatSummary for the specified other user.
    /// </summary>
    public static ChatSummary GetChatSummaryFor(string user)
    {
        if (mySocialData == null || mySocialData.chatSummary == null)
            return null;

        foreach (var cs in mySocialData.chatSummary.Values)
        {
            if (cs.otherUid == user)
                return cs;
        }
        return null;
    }

    /// <summary>
    /// Retrieves the ChatSummary with the specified id.
    /// </summary>
    public static ChatSummary GetChatSummaryId(string id)
    {
        if (mySocialData == null || mySocialData.chatSummary == null)
            return null;

        mySocialData.chatSummary.TryGetValue(id, out ChatSummary cs);
        return cs;
    }

    /// <summary>
    /// Truncates a string to a maximum length of 25 characters (adding ellipsis if needed).
    /// </summary>
    public static string TruncateString(string inputString)
    {
        if (inputString.Length > 25)
            return inputString.Substring(0, 25) + "...";
        return inputString;
    }

    /// <summary>
    /// Adds a new ChatSummary to the dictionary.
    /// </summary>
    public static void AddChatSummary(string id, string content, string otherUid)
    {
        if (mySocialData == null)
            return;

        if (mySocialData.chatSummary == null)
            mySocialData.chatSummary = new Dictionary<string, ChatSummary>();

        mySocialData.chatSummary[id] = new ChatSummary(id, TruncateString(content), otherUid);
    }

    /// <summary>
    /// Updates an existing ChatSummary identified by id.
    /// </summary>
    public static void UpdateChatSummary(string id, string content)
    {
        if (mySocialData == null)
            return;

        if (mySocialData.chatSummary == null)
            mySocialData.chatSummary = new Dictionary<string, ChatSummary>();

        if (mySocialData.chatSummary.TryGetValue(id, out ChatSummary cs))
        {
            cs.message = TruncateString(content);
            // Optionally trigger an event to update UI, etc.
        }
        else
        {
            // Create a new ChatSummary if it doesn't exist
            // Note: We need otherUid for a new ChatSummary, but we don't have it here
            // Using empty string as placeholder, consider modifying method signature if needed
            AddChatSummary(id, content, "");
        }
    }

    /// <summary>
    /// Helper function: converts a parsed JSON data (e.g., from a JSON library) into a List&lt;string&gt;.
    /// </summary>
    private static List<string> ParseToList(object data)
    {
        List<string> list = new List<string>();
        if (data is IEnumerable<object> enumerable)
        {
            foreach (var item in enumerable)
            {
                list.Add(item.ToString());
            }
        }
        return list;
    }


    /// <summary>
    /// Updates the static mySocialData instance using data from a dictionary.
    /// </summary>
    public static void UpdateMySocial(SocialData data)
    {
        mySocialData = data;
    }

    /// <summary>
    /// Returns true if the uid exists in myRequest.
    /// </summary>
    public static bool IsExistInMyRequest(string uid)
    {
        if (mySocialData == null || mySocialData.myRequest == null)
            return false;
        return mySocialData.myRequest.Contains(uid);
    }

    /// <summary>
    /// Returns true if the uid exists either in myRequest or friends.
    /// </summary>
    public static bool IsExitInMyRequestOrFriend(string uid)
    {
        return IsExistInMyRequest(uid) || IsExistInFriends(uid);
    }

    /// <summary>
    /// Returns true if the uid exists in friends.
    /// </summary>
    public static bool IsExistInFriends(string uid)
    {
        if (mySocialData == null || mySocialData.friends == null)
            return false;
        return mySocialData.friends.Contains(uid);
    }

    /// <summary>
    /// Adds a uid to the friends list.
    /// </summary>
    public static void AddFriend(string uid)
    {
        if (mySocialData == null)
            return;

        if (mySocialData.friends == null)
            mySocialData.friends = new List<string>();

        mySocialData.friends.Add(uid);
        // Trigger the OnFriendAdded event
        OnFriendAdded?.Invoke(uid);
    }

    /// <summary>
    /// Adds a uid to the myRequest list.
    /// </summary>
    public static void AddMyRequest(string uid)
    {
        if (mySocialData == null)
            return;

        if (mySocialData.myRequest == null)
            mySocialData.myRequest = new List<string>();

        mySocialData.myRequest.Add(uid);
    }

    /// <summary>
    /// Adds a timestamp entry for a uid in otherRequest.
    /// </summary>
    public static void AddOtherRequest(string uid, int timestamp)
    {
        if (mySocialData == null)
            return;

        if (mySocialData.otherRequest == null)
            mySocialData.otherRequest = new Dictionary<string, long>();

        mySocialData.otherRequest[uid] = timestamp;
    }

    /// <summary>
    /// Removes a uid from the friends list.
    /// </summary>
    public static void RemoveFriend(string uid)
    {
        if (mySocialData == null || mySocialData.friends == null)
            return;

        mySocialData.friends.Remove(uid);
    }

    /// <summary>
    /// Removes a uid from the myRequest list.
    /// </summary>
    public static void RemoveMyRequest(string uid)
    {
        if (mySocialData == null || mySocialData.myRequest == null)
            return;

        mySocialData.myRequest.Remove(uid);
    }

    /// <summary>
    /// Removes the otherRequest entry for the specified uid.
    /// </summary>
    public static void RemoveOtherRequest(string uid)
    {
        if (mySocialData == null || mySocialData.otherRequest == null)
            return;

        mySocialData.otherRequest.Remove(uid);
    }

    /// <summary>
    /// Logs detailed information about the SocialData instance to the Unity console.
    /// This includes friends, otherRequest, myRequest, and chatSummary data.
    /// </summary>
    public static void Log()
    {
        if (mySocialData == null)
        {
            Debug.LogWarning("SocialData.Log: mySocialData is null");
            return;
        }

        Debug.Log("=== SocialData Log ===");
        
        // Log friends
        Debug.Log("Friends (" + (mySocialData.friends?.Count ?? 0) + "):");
        if (mySocialData.friends != null && mySocialData.friends.Count > 0)
        {
            foreach (var friend in mySocialData.friends)
            {
                Debug.Log("  - " + friend);
            }
        }
        else
        {
            Debug.Log("  No friends found");
        }
        
        // Log otherRequest (friend requests from others)
        Debug.Log("Other Requests (" + (mySocialData.otherRequest?.Count ?? 0) + "):");
        if (mySocialData.otherRequest != null && mySocialData.otherRequest.Count > 0)
        {
            foreach (var request in mySocialData.otherRequest)
            {
                // Convert timestamp to readable date
                DateTime requestTime = DateTimeOffset.FromUnixTimeSeconds(request.Value).DateTime;
                Debug.Log($"  - From: {request.Key}, Time: {requestTime}");
            }
        }
        else
        {
            Debug.Log("  No other requests found");
        }
        
        // Log myRequest (friend requests sent by me)
        Debug.Log("My Requests (" + (mySocialData.myRequest?.Count ?? 0) + "):");
        if (mySocialData.myRequest != null && mySocialData.myRequest.Count > 0)
        {
            foreach (var request in mySocialData.myRequest)
            {
                Debug.Log("  - To: " + request);
            }
        }
        else
        {
            Debug.Log("  No outgoing requests found");
        }
        
        // Log chatSummary
        Debug.Log("Chat Summaries (" + (mySocialData.chatSummary?.Count ?? 0) + "):");
        if (mySocialData.chatSummary != null && mySocialData.chatSummary.Count > 0)
        {
            foreach (var chat in mySocialData.chatSummary)
            {
                Debug.Log($"  - ID: {chat.Key}, With: {chat.Value.otherUid}, Last Message: {chat.Value.message}");
            }
        }
        else
        {
            Debug.Log("  No chat summaries found");
        }
        
        Debug.Log("=== End SocialData Log ===");
    }

    /// <summary>
    /// Adds a friend request from another user
    /// </summary>
    public static void AddFriendRequest(string uid)
    {
        if (mySocialData == null)
            return;

        if (mySocialData.otherRequest == null)
            mySocialData.otherRequest = new Dictionary<string, long>();

        // Add the friend request with current timestamp if it doesn't exist
        if (!mySocialData.otherRequest.ContainsKey(uid))
        {
            mySocialData.otherRequest[uid] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}


[Serializable]
public class ConversationData
{
    // Static list to hold all conversation data instances.
    public static List<ConversationData> conversationDatas;

    public string id;
    public List<string> uids;
    public Dictionary<string, MessageData> messages;

    // Constructor
    public ConversationData(string id, List<string> uids, Dictionary<string, MessageData> messages)
    {
        this.id = id;
        this.uids = uids;
        this.messages = messages;
    }

    public void UpdateTimeConverstaiton(){
        foreach(var message in messages){
            message.Value.timestamp = long.Parse(message.Key);
        }
    }



    /// <summary>
    /// Finds and returns a ConversationData instance by its id from the static list.
    /// </summary>
    public static ConversationData FindConversationData(string convoId)
    {
        if (conversationDatas == null)
            return null;

        foreach (var convo in conversationDatas)
        {
            if (convo.id == convoId)
                return convo;
        }
        return null;
    }

    /// <summary>
    /// Loads conversation data asynchronously.
    /// The onComplete callback will be invoked with the loaded ConversationData.
    /// Assumes you have a LambdaHelper with a similar API.
    /// </summary>
    public static async UniTask<ConversationData> LoadConversationDataAsync(string convoId)
    {
        // Check if the conversation is already loaded.
        ConversationData found = FindConversationData(convoId);
        if (found != null)
            return found;

        // Await the LambdaAPI call.
        ServerRespone response = await LambdaAPI.LoadConvoData(convoId);

        if (!string.IsNullOrEmpty(response.error))
        {
            return null;
        }

        // Convert the JToken to a Dictionary.
        //Dictionary<string, object> data = response.jToken.ToObject<Dictionary<string, object>>();
        ConversationData convo = response.jToken.ToObject<ConversationData>();

        if (conversationDatas == null)
            conversationDatas = new List<ConversationData>();

        conversationDatas.Add(convo);
        return convo;
    }

    /// <summary>
    /// Adds a new conversation if it doesn't exist; otherwise, adds the message to the existing conversation.
    /// </summary>
    public static void AddNewConvo(string convoId, MessageData message, string otherUid)
    {
        if (conversationDatas == null)
            conversationDatas = new List<ConversationData>();

        ConversationData convo = FindConversationData(convoId);
        if (convo == null)
        {
            // Assuming you have a method to get your own uid, e.g., UserData.GetMyUid().
            List<string> uids = new List<string> { otherUid, UserData.currentUser.uid };
            Dictionary<string, MessageData> messages = new Dictionary<string, MessageData>();
            messages[message.timestamp.ToString()] = message;
            convo = new ConversationData(convoId, uids,  messages);
            conversationDatas.Add(convo);
        }
        else
        {
            convo.messages[message.timestamp.ToString()] = message;
        }
    }

    /// <summary>
    /// Adds a new message to an existing conversation.
    /// </summary>
    public static void AddNewMessage(string convoId, MessageData message, string otherUid)
    {
        if (conversationDatas == null)
            conversationDatas = new List<ConversationData>();

        ConversationData convo = FindConversationData(convoId);
        if (convo != null)
        {
            convo.messages[message.timestamp.ToString()] = message;
        }
        else
        {
            // Create a new conversation if not found

            
            List<string> uids = new List<string> { otherUid, UserData.currentUser.uid };
            Dictionary<string, MessageData> messages = new Dictionary<string, MessageData>();
            messages[message.timestamp.ToString()] = message;
            
            ConversationData newConvo = new ConversationData(convoId, uids, messages);
            conversationDatas.Add(newConvo);
            
            Debug.Log("Created new ConversationData for id: " + convoId);
        }
    }
}

/// <summary>
/// A simple helper class used for JSON conversion when the value is not already a string.
/// You may need to adjust this depending on your JSON parsing library.
/// </summary>
[Serializable]
public class SerializationWrapper
{
    public object value;
    public SerializationWrapper(object value)
    {
        this.value = value;
    }
}


#endregion