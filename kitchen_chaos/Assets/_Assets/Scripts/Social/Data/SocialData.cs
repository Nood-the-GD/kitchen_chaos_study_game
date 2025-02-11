using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Existing Message Classes

[Serializable]
public class MessageData
{
    public string content;
    public int timestamp;
    public string userId;

    // Constructor
    public MessageData(string content, int timestamp, string userId)
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
    public Dictionary<string, long> otherRequest;
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
    public static void UpdateMySocial(Dictionary<string, object> data)
    {
        mySocialData = FromDictionary(data);
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

#endregion