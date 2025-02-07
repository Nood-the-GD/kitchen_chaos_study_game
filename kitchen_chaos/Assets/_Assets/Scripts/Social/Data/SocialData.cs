using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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

[System.Serializable]
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