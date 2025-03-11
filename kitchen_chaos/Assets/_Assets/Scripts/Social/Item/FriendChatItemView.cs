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

    public ChatSummary chatSummary => SocialData.GetChatSummaryFor(otherUid);
    
    public string otherUid;
    public Button button;

    

    void Start(){
       
    }
    
    public void SetData(string otherUid,Action<FriendChatItemView> onClick)
    {  

        button.onClick.AddListener(()=>{
            onClick(this);
        });
        this.otherUid = otherUid;
        var chatSummary = SocialData.GetChatSummaryFor(otherUid);

        if(chatSummary != null){
            userName.text = "";
            message.text = chatSummary.message;
            Debug.Log("message with "+chatSummary.otherUid+" is: "+ message.text);
        }
        else{
            Debug.Log("message with "+otherUid+" is empty");
            message.text = "Say hi to your friend";
        }
        Init();
    }

    async void Init(){
        UserData user = await UserManager.GetUser(otherUid);
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
