using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class SetUserNamePopup : BasePopup<SetUserNamePopup>
{

    public GameObject closeButton;
    string userName;
    public void Start(){
        SetCloseButton(SaveData.isInited);
    }

    public void SetName(string name){
        userName = name;

    }

    public void Next(){
        UserData.currentUser.userName = userName;
        PhotonNetwork.NickName = userName;
        HidePopup();
    }

    

    public void SetCloseButton(bool isShow){
        closeButton.SetActive(isShow);
    }
}
