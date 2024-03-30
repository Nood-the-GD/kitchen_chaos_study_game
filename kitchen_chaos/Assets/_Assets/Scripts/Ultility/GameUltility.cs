using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public static class GameUtility
{
    public static GameObject SpawnMultiplay(this ObjectEnum objectEnum, Vector3 position = default, Quaternion rotation = default){
        return PhotonNetwork.Instantiate(GameData.s.GetPath(objectEnum), position, rotation);
    }
}
