using System;
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
    public string uid;
    public Button button;

    void Start(){
       
    }
    
    public void SetData(string uid,Action<FriendChatItemView> onClick)
    {  
        button.onClick.AddListener(()=>{
            onClick(this);
        });
        this.uid = uid;
        var chatSummary = SocialData.GetChatSummaryFor(uid);
        if(chatSummary != null){
            this.chatSummary = chatSummary;
            userName.text = "";
            message.text = chatSummary.content;
        }
        else{
            message.text = "Say hi to your friend";
        }
        Init();
    }

    async void Init(){
        UserData user = await UserManager.GetUser(uid);
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
