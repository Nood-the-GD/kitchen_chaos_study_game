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
        Debug.Log("Timestamp: " + data.timestamp);
        // Convert the Unix timestamp (in milliseconds) to a DateTime in local time
        DateTime localDateTime = DateTimeOffset.FromUnixTimeSeconds(data.timestamp).LocalDateTime;
        
        // Format the DateTime as "hh:mm tt" (12-hour format with AM/PM)
        this.time.text = localDateTime.ToString("hh:mm tt");
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
