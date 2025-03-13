using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoad : MonoBehaviour
{
    // Static instance that will hold the singleton reference
    private static DontDestroyOnLoad instance;

    // Property to access the instance from other scripts if needed
    public static DontDestroyOnLoad Instance
    {
        get { return instance; }
    }

    // Awake is called when the script instance is being loaded
    private void Awake()
    {

        // Retrieve the scene's name
        // if (UserData.currentUser == null && SceneManager.GetActiveScene().name != "AppConfigScene")
        // {
        //     SceneManager.LoadScene(SceneType.AppConfigScene.ToString());
        //     return;
        // }
        // Check if an instance already exists
        if (instance == null)
        {
            // If not, set this instance and make it persistent
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            // If an instance already exists and it's not this, destroy this duplicate
            DestroyImmediate(gameObject);
        }
    }

    void OnDisable()
    {
        Debug.Log("Destroy DontDestroyOnLoad");
    }
}
