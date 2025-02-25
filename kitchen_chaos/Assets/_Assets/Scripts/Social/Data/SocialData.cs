using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

[Serializable]
public class MessageDataChannel
{
    public string channel;
    public MessageData messageData;

    // Constructor
    public MessageDataChannel(string channel, MessageData messageData)
    {
        this.channel = channel;
        this.messageData = messageData;
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

    // Constructor
    public OtherRequest(string uid, int timestamp)
    {
        this.uid = uid;
        this.timestamp = timestamp;
    }

    /// <summary>
    /// Creates an instance from a dictionary.
    /// (In Unity you might parse JSON into a Dictionary via another JSON library.)
    /// </summary>
    public static OtherRequest FromDictionary(Dictionary<string, object> json)
    {
        string uid = json.ContainsKey("uid") ? json["uid"].ToString() : "";
        int timestamp = 0;
        if (json.ContainsKey("timestamp"))
        {
            object rawTimestamp = json["timestamp"];
            if (rawTimestamp is int)
            {
                timestamp = (int)rawTimestamp;
            }
            else if (rawTimestamp is long)
            {
                timestamp = Convert.ToInt32(rawTimestamp);
            }
            else if (rawTimestamp is string)
            {
                int.TryParse((string)rawTimestamp, out timestamp);
            }
        }
        return new OtherRequest(uid, timestamp);
    }

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
    public string content;
    public string otherUid;

    // Constructor
    public ChatSummary(string id, string content, string otherUid)
    {
        this.id = id;
        this.content = content;
        this.otherUid = otherUid;
    }

    /// <summary>
    /// Creates an instance from a dictionary.
    /// Expects the dictionary to have keys "message" and "otherUid".
    /// </summary>
    public static ChatSummary FromDictionary(string id, Dictionary<string, object> json)
    {
        string content = json.ContainsKey("message") ? json["message"].ToString() : "";
        string otherUid = json.ContainsKey("otherUid") ? json["otherUid"].ToString() : "";
        return new ChatSummary(id, content, otherUid);
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
        if (mySocialData == null || mySocialData.chatSummary == null)
            return;

        if (mySocialData.chatSummary.TryGetValue(id, out ChatSummary cs))
        {
            cs.content = TruncateString(content);
            // Optionally trigger an event to update UI, etc.
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
    /// Creates a SocialData instance from a dictionary.
    /// Expected keys: "friends", "otherRequest", "chatSummary", and "myRequest".
    /// </summary>
    public static SocialData FromDictionary(Dictionary<string, object> json)
    {
        // Parse friends.
        List<string> friends = json.ContainsKey("friends") ? ParseToList(json["friends"]) : new List<string>();

        // Parse otherRequest: expected as a dictionary where each key is a uid and value is a timestamp.
        Dictionary<string, long> otherRequest = new Dictionary<string, long>();
        if (json.ContainsKey("otherRequest") && json["otherRequest"] is Dictionary<string, object> otherReqDict)
        {
            foreach (var entry in otherReqDict)
            {
                int timestamp = 0;
                if (entry.Value is int)
                    timestamp = (int)entry.Value;
                else if (entry.Value is long)
                    timestamp = Convert.ToInt32(entry.Value);
                else if (entry.Value is string)
                    int.TryParse((string)entry.Value, out timestamp);

                otherRequest[entry.Key] = timestamp;
            }
        }

        // Parse chatSummary: expected as a dictionary where the key is an id and the value is another dictionary with details.
        Dictionary<string, ChatSummary> chatSummary = new Dictionary<string, ChatSummary>();
        if (json.ContainsKey("chatSummary") && json["chatSummary"] is Dictionary<string, object> chatSummaryDict)
        {
            foreach (var entry in chatSummaryDict)
            {
                if (entry.Value is Dictionary<string, object> csData)
                {
                    chatSummary[entry.Key] = ChatSummary.FromDictionary(entry.Key, csData);
                }
            }
        }

        // Parse myRequest.
        List<string> myRequest = json.ContainsKey("myRequest") ? ParseToList(json["myRequest"]) : new List<string>();

        return new SocialData(friends, otherRequest, myRequest, chatSummary);
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
        // Optionally, trigger an event or update UI.
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
}


[Serializable]
public class ConversationData
{
    // Static list to hold all conversation data instances.
    public static List<ConversationData> conversationDatas;

    public string id;
    public List<string> uids;
    public List<MessageData> messageData;

    // Constructor
    public ConversationData(string id, List<string> uids, List<MessageData> messageData)
    {
        this.id = id;
        this.uids = uids;
        this.messageData = messageData;
    }

    /// <summary>
    /// Creates an instance of ConversationData from a dictionary.
    /// Expects keys: "id", "users" (as List or array of strings), and "messages" (a Dictionary with keys as string timestamps).
    /// </summary>
    public static ConversationData FromDictionary(Dictionary<string, object> json)
    {
        // Get the conversation id.
        string id = json["id"].ToString();

        // Parse the list of user ids.
        List<string> uids = new List<string>();
        if (json.ContainsKey("users") && json["users"] is IEnumerable<object> users)
        {
            foreach (var user in users)
            {
                uids.Add(user.ToString());
            }
        }

        // Parse messages. Expected to be a Dictionary<string, object>.
        List<MessageData> messagesList = new List<MessageData>();
        if (json.ContainsKey("messages") && json["messages"] is Dictionary<string, object> messages)
        {
            foreach (var entry in messages)
            {
                // The key is expected to be a string representing an int (like "12345").
                if (int.TryParse(entry.Key, out int timestamp))
                {
                    // Each value is a JSON string or a Dictionary that can be converted to JSON.
                    // Here, we assume it's a Dictionary<string, object> that you then convert to a JSON string.
                    // You might need to use your JSON library here; for simplicity, we'll assume a helper method:
                    // e.g., JsonHelper.ToJson(entry.Value)
                    string jsonStr = JsonUtility.ToJson(new SerializationWrapper(entry.Value));
                    MessageData message = MessageData.FromJson2(timestamp, jsonStr);
                    messagesList.Add(message);
                }
                else
                {
                    Debug.LogWarning("Could not parse message key as int: " + entry.Key);
                }
            }
        }

        return new ConversationData(id, uids, messagesList);
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
        Dictionary<string, object> data = response.jToken.ToObject<Dictionary<string, object>>();
        ConversationData convo = FromDictionary(data);

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
            convo = new ConversationData(convoId, uids, new List<MessageData> { message });
            conversationDatas.Add(convo);
        }
        else
        {
            convo.messageData.Add(message);
        }
    }

    /// <summary>
    /// Adds a new message to an existing conversation.
    /// </summary>
    public static void AddNewMessage(string convoId, MessageData message)
    {
        if (conversationDatas == null)
            conversationDatas = new List<ConversationData>();

        ConversationData convo = FindConversationData(convoId);
        if (convo != null)
        {
            convo.messageData.Add(message);
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