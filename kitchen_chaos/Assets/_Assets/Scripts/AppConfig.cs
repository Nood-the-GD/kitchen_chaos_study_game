using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//-----------------------------------------------------------------------------
// ConfigTask class: Holds information about each initialization task
//-----------------------------------------------------------------------------
public class ConfigTask
{
    public string name;
    public bool autoRetry;
    public int maxRetries;
    // The task delegate takes a callback to signal success (true) or failure (false)
    public System.Func<System.Action<bool>, IEnumerator> task;

    public ConfigTask(string name, System.Func<System.Action<bool>, IEnumerator> task, bool autoRetry = false, int maxRetries = 3)
    {
        this.name = name;
        this.task = task;
        this.autoRetry = autoRetry;
        this.maxRetries = maxRetries;
    }
}

//-----------------------------------------------------------------------------
// AppConfig: Main initializer using a task system similar to the Flutter version.
//-----------------------------------------------------------------------------
public class AppConfig : MonoBehaviour
{
    bool initApp = false;
    List<ConfigTask> tasks;      // List of configuration tasks
    int completedTasks = 0;      // Number of tasks successfully completed

    // UI references (assign these in the Inspector)
    public Image progressBar;    // An Image that acts as a slider (with its type set to Filled)
    public Text progressText;    // Displays percentage of tasks completed
    public Text currentTaskText; // Displays the name of the current task

    void Start()
    {
        // If the user data is not initialized, show a popup.
        if (!SaveData.isInited)
        {
            var popup = SetUserNamePopup.ShowPopup();
            popup.isCreateUser = true;
        }

        // Initialize UI elements
        if (progressBar != null)
        {
            progressBar.fillAmount = 0;
        }
        if (progressText != null)
        {
            progressText.text = "0% Completed";
        }
        if (currentTaskText != null)
        {
            currentTaskText.text = "";
        }
    }

    // Check every FixedUpdate if the user is initialized before starting the app
    private void FixedUpdate()
    {
        // Once we've begun initialization, do nothing further.
        if (initApp)
            return;

        if (SaveData.isInited)
        {
            initApp = true;
            StartCoroutine(InitApp());
        }
    }

    // Sets up and runs the initialization tasks
    IEnumerator InitApp()
    {
        // Define your list of tasks. Replace DummyTask with your actual task logic.
        tasks = new List<ConfigTask>()
        {
            new ConfigTask(
                "Translating",
                (callback) => DummyTask(0.1f, true, callback)
            ),
            new ConfigTask(
                "Loggin",
                (callback) => LoginTask(0.1f, true, callback)
            )
        };

        // Run tasks sequentially while updating the UI with tweened progress
        yield return StartCoroutine(RunTasks());
    }

    // Runs each task one-by-one. Stops if any task fails after all retries.
    IEnumerator RunTasks()
    {
        foreach (var task in tasks)
        {
            // Update current task text
            if (currentTaskText != null)
            {
                currentTaskText.text = "Executing: " + task.name;
            }
            Debug.Log("Starting task: " + task.name);

            bool success = false;
            yield return StartCoroutine(ExecuteTaskWithRetry(task, (result) => {
                success = result;
            }));

            if (!success)
            {
                Debug.Log("Task failed: " + task.name);
                if (currentTaskText != null)
                {
                    currentTaskText.text = task.name + " failed.";
                }
                // Optionally, implement UI for manual retry or error display here.
                yield break;
            }
            else
            {
                Debug.Log("Task completed: " + task.name);
                completedTasks++;

                // Calculate the target fill amount based on completed tasks
                float targetFill = (float)completedTasks / tasks.Count;

                // Tween the progress bar fill from its current value to the target value over 0.2 seconds
                if (progressBar != null)
                {
                    yield return StartCoroutine(AnimateProgressBar(progressBar.fillAmount, targetFill, 0.2f));
                }
                if (progressText != null)
                {
                    progressText.text = string.Format("{0}% Completed", (int)(targetFill * 100));
                }
            }
        }

        // All tasks completed, move to the next scene
        onDoneInitApp();
    }

    // Executes an individual task with retry logic if enabled.
    IEnumerator ExecuteTaskWithRetry(ConfigTask task, System.Action<bool> callback)
    {
        int retryCount = 0;
        bool success = false;

        while (true)
        {
            yield return StartCoroutine(task.task((bool result) => {
                success = result;
            }));

            if (success)
            {
                callback(true);
                yield break;
            }
            else
            {
                if (task.autoRetry && retryCount < task.maxRetries)
                {
                    retryCount++;
                    Debug.Log("Retrying " + task.name + " (Attempt " + retryCount + ")...");
                    if (currentTaskText != null)
                    {
                        currentTaskText.text = "Retrying " + task.name + " (Attempt " + retryCount + ")";
                    }
                    yield return new WaitForSeconds(2); // Wait 2 seconds before retrying
                    continue;
                }
                else
                {
                    callback(false);
                    yield break;
                }
            }
        }
    }

    // A dummy task that simulates work by waiting a short time before reporting success.
    // Replace this with your actual task logic.
    IEnumerator DummyTask(float delay, bool result, System.Action<bool> callback)
    {
        yield return new WaitForSeconds(delay);
        callback(result);
    }

    IEnumerator LoginTask(float delay, bool result, System.Action<bool> callback ){
        yield return new WaitForSeconds(delay);
        if(UserData.currentUser != null){
            callback(true);
            yield break;
        }

        yield return LambdaAPI.TryLogin(SaveData.userId, SaveData.userToken, (response) => {
            if(response != null){
                UserData.SetCurrentUser(response.ToObject<UserData>());
                callback(true);
            }else{
                
                callback(false);
            }
        },(error)=>{
            callback(false);
        });
    }

    // Tween the progress bar fill amount over a given duration.
    IEnumerator AnimateProgressBar(float fromValue, float toValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newFill = Mathf.Lerp(fromValue, toValue, elapsed / duration);
            progressBar.fillAmount = newFill;
            yield return null;
        }
        progressBar.fillAmount = toValue;
    }

    // Called when all tasks have been successfully completed.
    void onDoneInitApp()
    {
        // Option 1: Load a different scene (recommended)
        // Replace "MainMenuScene" with the name of your target scene.
        Debug.Log("Going to MainMenuScene...");
        SceneManager.LoadScene(SceneType.MainMenuScene.ToString());

        // Option 2: If you intend to stay in the same scene, disable this script to avoid re-running
        // this.enabled = false;
    }
}
