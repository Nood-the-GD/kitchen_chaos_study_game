using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AppManager : MonoBehaviour
{
    async void Start(){
        PhotonManager.s.Init();
        var p = await LambdaAPI.GetMySocial();
        SocialData.mySocialData = p.jToken.ToObject<SocialData>(); 
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
#endregion
}
