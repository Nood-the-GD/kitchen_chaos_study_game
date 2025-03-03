using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using DG;
public static class GameUtility
{
    public static GameObject SpawnMultiplay(this ObjectEnum objectEnum, Vector3 position = default, Quaternion rotation = default){
        return PhotonNetwork.Instantiate(GameData.s.GetPath(objectEnum), position, rotation);
    }

    public static GameObject SpawnMultiplay(this string objectName, Vector3 position = default, Quaternion rotation = default){
        return PhotonNetwork.Instantiate(GameData.s.GetPath(objectName), position, rotation);
    }

}


public class RandomStringGenerator
{
    // Function to generate a 7-digit random number
    public static string GenerateRandomString(int length)
    {
        // Use current time to seed the random number generator
        var seed = (int)DateTime.Now.Ticks;
        var random = new System.Random(seed);

        // Generate a random number with 'length' digits
        string result = "";
        for (int i = 0; i < length; i++)
        {
            result += random.Next(0, 10).ToString();
        }

        return result;
    }
}
