using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
public static class GameUtility
{
    public static GameObject SpawnMultiplay(this ObjectEnum objectEnum, Vector3 position = default, Quaternion rotation = default){
        return PhotonNetwork.Instantiate(GameData.s.GetPath(objectEnum), position, rotation);
    }

}


public class RandomStringGenerator
{
    // Function to generate a 7-character random string based on current time
    public static string GenerateRandomString(int maxChar)
    {
        // Use current time to seed the random number generator
        var seed = (int)DateTime.Now.Ticks;
        var random = new System.Random(seed);

        string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var charsArr = new char[maxChar];

        for (int i = 0; i < charsArr.Length; i++)
        {
            charsArr[i] = characters[random.Next(characters.Length)];
        }

        return new string(charsArr);
    }
}
