using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FriendChatItemView : MonoBehaviour
{   
    public Text userName;
    public Text message;
    public Image activeStatus;
    public ChatSummary chatSummary;
    
    public void SetData(ChatSummary chatSummary)
    {
        this.chatSummary = chatSummary;
        userName.text = "";
        message.text = chatSummary.content;
        Init();
    }

    async void Init(){
        UserData user = await UserManager.GetUser(chatSummary.otherUid);
        userName.text = user.username;
        if(user.activeStatus == "online"){
            activeStatus.color = Color.green;
        }
        if(user.activeStatus == "offline"){
            activeStatus.color = Color.gray;
        }
        if(user.activeStatus == "inGame"){
            activeStatus.color = Color.yellow;
        }
    }

}
