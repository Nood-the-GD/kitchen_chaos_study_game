using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIElement : MonoBehaviour
{
    // Start is called before the first frame update
    public Text userName;
    public Photon.Realtime.Player player;
    public void SetData(Photon.Realtime.Player player){
        userName.text = player.NickName;
        this.player = player;
    }
}
