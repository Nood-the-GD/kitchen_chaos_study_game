using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AppManager : MonoBehaviour
{
    private void OnEnable()
    {

        var currentScene = SceneManager.GetActiveScene();
        
        // Retrieve the scene's name
        string sceneName = currentScene.name;
        if(sceneName == SceneType.MainMenuScene.ToString())
        {
            SceneManager.LoadScene(SceneType.AppConfigScene.ToString());
        }
    }

    private void OnDisable() {
        ServerConnect.Dispose();
    }
}
