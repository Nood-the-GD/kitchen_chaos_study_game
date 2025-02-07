using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    private void OnEnable()
    {
                if(!SaveData.isInited)
        {
            SetUserNamePopup.ShowPopup();
        }
        if(SaveData.isInited){
          connectServer();
        }
    }

    async void connectServer(){
        await ServerConnect.ConnectServer();
    }

    private void OnDisable() {
        ServerConnect.Dispose();
    }
}
