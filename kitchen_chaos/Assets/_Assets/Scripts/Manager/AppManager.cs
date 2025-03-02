using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AppManager : MonoBehaviour
{
    public SocialData debugSocialData;
    async void Start(){
        Debug.Log("AppManager Start");
        PhotonManager.s.Init();
        var p = await LambdaAPI.GetMySocial();
        SocialData.mySocialData = p.jToken.ToObject<SocialData>(); 
        SocialData.mySocialData.SetUp();
        debugSocialData = SocialData.mySocialData;
        await ServerConnect.ConnectServer();
    }

    

    private void OnDisable() {
        ServerConnect.Dispose();
    }

#region  Editor
    [Button]
    void ClearCurrentUser(){
        UserData.currentUser = null;
    }

    [Button]
    void ClearAllKey(){
        PlayerPrefs.DeleteAll();
    }

    [Button]
    void CreateUser(){
        LambdaAPI.CreateUser("test", "male");
    }

    [Button]
    void AcceptFriend(string fromUid, string otherUid){
        LambdaAPI.AcceptFriendCustom(fromUid, otherUid);
    }

    [Button]
    void ClearConversation(){
        ConversationData.conversationDatas = new List<ConversationData>();
    }

    [Button]
    void TestStream(){
        LambdaAPI.TestStream();
    }
#endregion
}
