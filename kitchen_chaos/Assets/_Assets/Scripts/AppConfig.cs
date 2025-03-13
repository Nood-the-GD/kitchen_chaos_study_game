using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

//
// ConfigTask class: Holds information about each initialization task.
//
public class ConfigTask
{
    public string name;
    public bool autoRetry;
    public int maxRetries;
    // The task delegate now returns a UniTask<bool> directly.
    public Func<UniTask<bool>> task;

    public ConfigTask(string name, Func<UniTask<bool>> task, bool autoRetry = false, int maxRetries = 3)
    {
        this.name = name;
        this.task = task;
        this.autoRetry = autoRetry;
        this.maxRetries = maxRetries;
    }
}

//
// AppConfig: Main initializer using a task system (converted to UniTask).
//
public class AppConfig : MonoBehaviour
{
    bool initApp = false;
    List<ConfigTask> tasks;      // List of configuration tasks
    int completedTasks = 0;      // Number of tasks successfully completed

    // UI references (assign these in the Inspector)
    public Image progressBar;     // An Image that acts as a slider (set its Image Type to Filled)
    public Text progressText;     // Displays percentage of tasks completed
    public Text currentTaskText;  // Displays the name of the current task
    public GameObject initObject;

    void Start()
    {
        // If the user data is not initialized, show a popup.
        if (!SaveData.isInited)
        {
            var popup = SetUserNamePopup.ShowPopup();
            popup.isCreateUser = true;
        }

        // Initialize UI elements.
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

    // Check every FixedUpdate if the user is initialized before starting the app.
    private void FixedUpdate()
    {
        // Once we've begun initialization, do nothing further.
        if (initApp)
            return;

        if (SaveData.isInited)
        {
            initApp = true;
            // Start the async initialization flow (fire-and-forget).
            RunInitAppAsync().Forget();
        }
    }

    // Sets up and runs the initialization tasks asynchronously.
    async UniTaskVoid RunInitAppAsync()
    {
        // Define your list of tasks. Replace DummyTaskAsync with your actual task logic.
        tasks = new List<ConfigTask>()
        {
            new ConfigTask(
                "Translating",
                () => DummyTaskAsync(0.1f, true)
            ),
            new ConfigTask(
                "Loggin",
                () => LoginTaskAsync(0.1f, true)
            )
        };

        // Run tasks sequentially while updating the UI.
        await RunTasksAsync();
    }

    // Runs each task one-by-one. Stops if any task fails after all retries.
    async UniTask RunTasksAsync()
    {
        foreach (var task in tasks)
        {
            // Update current task text.
            if (currentTaskText != null)
            {
                currentTaskText.text = "Executing: " + task.name;
            }
            Debug.Log("Starting task: " + task.name);

            bool success = await ExecuteTaskWithRetryAsync(task);

            if (!success)
            {
                Debug.Log("Task failed: " + task.name);
                if (currentTaskText != null)
                {
                    currentTaskText.text = task.name + " failed.";
                }
                // Optionally, implement UI for manual retry or error display here.
                return;
            }
            else
            {
                Debug.Log("Task completed: " + task.name);
                completedTasks++;

                // Calculate the target fill amount based on completed tasks.
                float targetFill = (float)completedTasks / tasks.Count;

                // Tween the progress bar fill from its current value to the target value over 0.2 seconds.
                if (progressBar != null)
                {
                    await AnimateProgressBarAsync(progressBar.fillAmount, targetFill, 0.2f);
                }
                if (progressText != null)
                {
                    progressText.text = string.Format("{0}% Completed", (int)(targetFill * 100));
                }
            }
        }

        // All tasks completed, move to the next scene.
        onDoneInitApp();
    }

    // Executes an individual task with retry logic if enabled.
    async UniTask<bool> ExecuteTaskWithRetryAsync(ConfigTask task)
    {
        int retryCount = 0;
        while (true)
        {
            bool success = await task.task();
            if (success)
            {
                return true;
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
                    await UniTask.Delay(TimeSpan.FromSeconds(2)); // Wait 2 seconds before retrying.
                    continue;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    // A dummy task that simulates work by waiting a short time before reporting success.
    // Replace this with your actual task logic.
    async UniTask<bool> DummyTaskAsync(float delay, bool result)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        return result;
    }

    // The login task, converted from coroutine to async/await.
    async UniTask<bool> LoginTaskAsync(float delay, bool dummyResult)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        if (UserData.currentUser != null)
        {
            return true;
        }

        // Wrap the callback-based LambdaAPI.TryLogin call into a UniTask.
        var tcs = new UniTaskCompletionSource<bool>();

        var p = await LambdaAPI.TryLogin(
            SaveData.userId,
            SaveData.userToken
        );

        if (p.IsError)
        {
            tcs.TrySetResult(false);
        }
        else
        {
            UserData.SetCurrentUser(p.jToken.ToObject<UserData>());
            tcs.TrySetResult(true);
        }


        return await tcs.Task;
    }

    // Tween the progress bar fill amount over a given duration.
    async UniTask AnimateProgressBarAsync(float fromValue, float toValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newFill = Mathf.Lerp(fromValue, toValue, elapsed / duration);
            if (progressBar != null)
            {
                progressBar.fillAmount = newFill;
            }
            // Wait until the next frame.
            await UniTask.Yield();
        }
        if (progressBar != null)
        {
            progressBar.fillAmount = toValue;
        }
    }

    // Called when all tasks have been successfully completed.
    void onDoneInitApp()
    {
        // Option 1: Load a different scene (recommended)
        // Replace "MainMenuScene" with the name of your target scene.
        Debug.Log("Going to MainMenuScene...");
        initObject.SetActive(true);
        SceneManager.LoadScene(SceneType.MainMenuScene.ToString());
        // AfterInitApp();
        // Option 2: If you intend to stay in the same scene, disable this script to avoid re-running.
        // this.enabled = false;
    }

    async void AfterInitApp()
    {
        await UniTask.WaitUntil(() => SceneManager.GetActiveScene().name == SceneType.MainMenuScene.ToString());
        initObject.SetActive(true);
    }
}
