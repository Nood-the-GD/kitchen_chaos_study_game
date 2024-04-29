using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSetting{
    public static bool volume {
        get => PlayerPrefs.GetInt("volume", 1) == 1;
        set => PlayerPrefs.SetInt("volume", value ? 1 : 0);
    }
    public static ColorSkin colorSkin{
        get{
            var colorCode = PlayerPrefs.GetString("colorSkin", "blue");
            return GameData.s.colorElements.Find(x => x.colorCode == colorCode);
        }
        set => PlayerPrefs.SetString("colorSkin", value.colorCode);
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
