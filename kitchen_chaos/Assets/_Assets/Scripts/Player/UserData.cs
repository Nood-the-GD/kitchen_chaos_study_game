using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UserData
{
    // public static UserData myPlayerData{
    //     get =>
    // }
    public static bool isInitName => PlayerPrefs.HasKey("userName");
    public static string userName{
        get => PlayerPrefs.GetString("userName", "Player");
        set => PlayerPrefs.SetString("userName", value);
    }
}
