using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserSetting
{
    public static bool volume
    {
        get => PlayerPrefs.GetInt("volume", 1) == 1;
        set => PlayerPrefs.SetInt("volume", value ? 1 : 0);
    }
    public static ColorSkin colorSkin
    {
        get
        {
            var colorCode = PlayerPrefs.GetString("colorSkin", "blue");
            return GameData.s.colorElements.Find(x => x.colorCode == colorCode);
        }
        set => PlayerPrefs.SetString("colorSkin", value.colorCode);
    }

    public static string regionSelected
    {
        get => PlayerPrefs.GetString("regionSelected", "null");
        set => PlayerPrefs.SetString("regionSelected", value);
    }
}

public class SaveData{
    public static string userId 
    {
        get => PlayerPrefs.GetString("userId", "null");
        set => PlayerPrefs.SetString("userId", value);
    }

    public static string userToken{
        get => PlayerPrefs.GetString("userToken", "null");
        set => PlayerPrefs.SetString("userToken", value);
    }

    public static bool isInited{
        get => PlayerPrefs.HasKey("userId");
    }
}

[System.Serializable]
public class UserData
{
    public String userName;
    public String userGender;
    public String uid;

    public static UserData currentUser;

    public static void SetCurrentUser(UserData user){
        currentUser = user;
    }
}
