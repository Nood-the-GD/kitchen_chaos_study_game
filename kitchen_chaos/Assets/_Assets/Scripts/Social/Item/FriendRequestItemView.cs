using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FriendRequestItemView : MonoBehaviour
{
    public Button decline;
    public Button addFriend;
    public Text userName;
    public UserData userData;
    public async void SetData(string uid){
        Debug.Log("item view: "+ uid);
        var p = await UserManager.GetUser(uid);
        this.userData = p;
        userName.text = userData.username;
    }



    public void Start(){
        decline.onClick.AddListener(OnDecline);
        addFriend.onClick.AddListener(OnAddFriend);
    }

    async void OnDecline(){
        await LambdaAPI.DeclineFriend(userData.uid);
        Destroy(gameObject);
    }

    async void OnAddFriend(){
        var p =await LambdaAPI.AcceptFriend(userData.uid);
        if(p.IsSuccess){
            SocialData.AddFriend(userData.uid);
            SocialData.RemoveOtherRequest(userData.uid);
            FriendRequestPopup.HidePopup();
        }
        
        Destroy(gameObject);
    }
}
