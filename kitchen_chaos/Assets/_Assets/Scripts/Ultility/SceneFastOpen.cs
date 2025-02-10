using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SceneFastOpen
{
    [MenuItem("SceneFastOpen/OpenAppConfigScene")]
    public static void OpenAppConfigScene()
    {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Assets/Scenes/AppConfigScene.unity");
#endif
    }

    [MenuItem("SceneFastOpen/OpenMainScene")]
    public static void OpenMainScene()
    {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Assets/Scenes/MainMenuScene.unity");
#endif
    }



    [MenuItem("SceneFastOpen/OpenGameScene")]
    public static void OpenGameScene()
    {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Assets/Scenes/GameScene.unity");
#endif
    }



}
