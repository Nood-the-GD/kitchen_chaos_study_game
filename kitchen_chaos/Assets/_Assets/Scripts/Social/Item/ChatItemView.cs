using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public class ChatItemView : MonoBehaviour
{
    public Text userName;
    public Text message;
    public Text time;
    public HorizontalLayoutGroup layoutGroup;

    
    public async void SetData(MessageData data)
    {

        layoutGroup = GetComponent<HorizontalLayoutGroup>();



        this.message.text = data.content;

        // Convert the Unix timestamp (in milliseconds) to a DateTime in local time
        DateTime localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(data.timestamp).LocalDateTime;
        
        // Format the DateTime as "hh:mm" (12-hour format) or "HH:mm" for 24-hour format.
        // Here we'll use "hh:mm" as requested.
        this.time.text = localDateTime.ToString("hh:mm");
        var p = await UserManager.GetUser(data.userId);
        this.userName.text = p.username;

        var isMine = UserData.IsMineId(data.userId);
        if(isMine){
            layoutGroup.childAlignment = TextAnchor.UpperRight;
            userName.alignment = TextAnchor.UpperRight;
            time.alignment = TextAnchor.UpperLeft;
        }
        else{
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            userName.alignment = TextAnchor.UpperLeft;
            time.alignment = TextAnchor.UpperRight;
        }


    }


}
