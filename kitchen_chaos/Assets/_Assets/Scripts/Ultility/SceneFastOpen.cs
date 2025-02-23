#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SceneFastOpen
{
    [MenuItem("SceneFastOpen/OpenAppConfigScene")]
    public static void OpenAppConfigScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Assets/Scenes/AppConfigScene.unity");
    }

    [MenuItem("SceneFastOpen/OpenMainScene")]
    public static void OpenMainScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Assets/Scenes/MainMenuScene.unity");
    }



    [MenuItem("SceneFastOpen/OpenGameScene")]
    public static void OpenGameScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Assets/Scenes/GameScene.unity");
    }
}
#endif
