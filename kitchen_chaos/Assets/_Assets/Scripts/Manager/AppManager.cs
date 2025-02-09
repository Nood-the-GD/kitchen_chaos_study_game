using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AppManager : MonoBehaviour
{
    void Start(){
        PhotonManager.s.Init();
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
