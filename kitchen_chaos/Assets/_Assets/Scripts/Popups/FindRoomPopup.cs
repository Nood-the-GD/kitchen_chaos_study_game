using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class FindRoomPopup : BasePopup<FindRoomPopup>
{
    public string curRoomName;
    [SerializeField] private Button _findRoomBtn;
    [SerializeField] private InputField _inputRoomNameField;

    void Awake() 
    {
        _findRoomBtn.onClick.AddListener(FindRoom);        
    }

    public void SetRoomName(string name){
        curRoomName = name;
    }
    public void FindRoom(){
        string roomName = _inputRoomNameField.text.Trim();
        PhotonNetwork.JoinRoom(roomName);
    }
}
