using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ChatItemView : MonoBehaviour
{
    public Text userName;
    public Text message;
    public Text time;

    public async void SetData(MessageData data)
    {
        var p = await UserManager.GetUser(data.userId);
        this.userName.text = p.username;
        this.message.text = data.content;

        // Convert the Unix timestamp (in milliseconds) to a DateTime in local time
        DateTime localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).LocalDateTime;
        
        // Format the DateTime as "hh:mm" (12-hour format) or "HH:mm" for 24-hour format.
        // Here we'll use "hh:mm" as requested.
        this.time.text = localDateTime.ToString("hh:mm");
    }

}
