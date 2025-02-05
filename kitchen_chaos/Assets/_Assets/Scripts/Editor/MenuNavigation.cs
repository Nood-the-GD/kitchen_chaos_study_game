using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MenuNavigation : EditorWindow
{
    private static List<string> _buildScenePath = new List<string>();

    [MenuItem("Tools/Open Scene")]
    public static void OpenMenu()
    {
        GetBuildScenes();
        EditorWindow.CreateInstance<MenuNavigation>().Show();
    }

    void OnEnable()
    {
        GetBuildScenes();
    }

    public void OnGUI()
    {
        if (_buildScenePath.Count == 0)
        {
            EditorGUILayout.LabelField("No scenes found in build settings.");
            return;
        }

        for (int i = 0; i < _buildScenePath.Count; i++)
        {
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(_buildScenePath[i]);
            if (GUILayout.Button(sceneName))
            {
                OpenScene(i);
            }
        }
    }

    private static void OpenScene(int index)
    {
        if (index >= 0 && index < _buildScenePath.Count)
        {
            EditorSceneManager.OpenScene(_buildScenePath[index]);
        }
    }

    private static void GetBuildScenes()
    {
        _buildScenePath = EditorBuildSettings.scenes.Select(scene => scene.path).ToList();
    }

}
