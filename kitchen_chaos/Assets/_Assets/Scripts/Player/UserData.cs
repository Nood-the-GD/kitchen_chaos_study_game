using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSetting{
    public static bool volume {
        get => PlayerPrefs.GetInt("volume", 1) == 1;
        set => PlayerPrefs.SetInt("volume", value ? 1 : 0);
    }

}

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
