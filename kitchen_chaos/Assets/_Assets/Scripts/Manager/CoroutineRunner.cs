using UnityEngine;
using System.Collections;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find an existing instance in the scene.
                _instance = FindObjectOfType<CoroutineRunner>();
                if (_instance == null)
                {
                    // Create a new GameObject if none exists.
                    GameObject go = new GameObject("CoroutineRunner");
                    _instance = go.AddComponent<CoroutineRunner>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
}
