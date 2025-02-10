using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq; // Needed to work with JToken

public static class UserManager
{
    // In-memory cache of users.
    private static List<UserData> users = new List<UserData>();

    // Dictionary to track ongoing fetch requests.
    // Key: uid, Value: Task that will eventually return a UserData.
    private static Dictionary<string, Task<UserData>> _pendingRequests = new Dictionary<string, Task<UserData>>();

    // Maximum number of concurrent pending requests.
    private const int _maxPendingRequests = 10;

    // Delay duration if max pending is reached.
    private static readonly TimeSpan _pendingWaitDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Attempts to return the cached user for the specified uid.
    /// If not found, fetches from the backend.
    /// </summary>
    public static async Task<UserData> GetUser(string uid)
    {
        // Try to find the user in the cache.
        UserData existingUser = users.FirstOrDefault(user => user.uid == uid);
        if (existingUser != null)
        {
            return existingUser;
        }

        // Check if there's an ongoing fetch for this uid.
        if (_pendingRequests.TryGetValue(uid, out Task<UserData> pendingTask))
        {
            return await pendingTask;
        }

        // Wait if too many pending requests are active.
        while (_pendingRequests.Count >= _maxPendingRequests)
        {
            await Task.Delay(_pendingWaitDuration);
        }

        // Start a new fetch request.
        Task<UserData> fetchTask = FetchUserFromBackend(uid);
        _pendingRequests[uid] = fetchTask;

        try
        {
            UserData user = await fetchTask;
            return user;
        }
        finally
        {
            // Remove the pending task regardless of success or failure.
            _pendingRequests.Remove(uid);
        }
    }

    /// <summary>
    /// Updates the cache with the provided user data.
    /// </summary>
    public static void UpdateUser(UserData user)
    {
        // Remove any existing user with the same uid.
        users.RemoveAll(existingUser => existingUser.uid == user.uid);
        // Add the new user data.
        users.Add(user);
    }

    /// <summary>
    /// Returns the username of the cached user with the specified uid.
    /// If no user is found, an empty string is returned.
    /// </summary>
    public static string ForceGetUserName(string uid)
    {
        UserData user = users.FirstOrDefault(u => u.uid == uid);
        return user != null ? user.username : "";
    }

    /// <summary>
    /// Returns the cached user for the specified uid.
    /// Returns null if the user is not in the cache.
    /// </summary>
    public static UserData ForceGetUser(string uid)
    {
        return users.FirstOrDefault(u => u.uid == uid);
    }

    /// <summary>
    /// Private method to fetch user data from the backend using LambdaAPI.
    /// This version uses a coroutine and returns a Task that completes with a UserData instance.
    /// </summary>
    private static Task<UserData> FetchUserFromBackend(string uid)
    {
        var tcs = new TaskCompletionSource<UserData>();
        CoroutineRunner.Instance.StartCoroutine(FetchUserCoroutine(uid, tcs));
        return tcs.Task;
    }

    /// <summary>
    /// Coroutine that calls LambdaAPI.GetUser and sets the TaskCompletionSource when done.
    /// </summary>
    private static IEnumerator FetchUserCoroutine(string uid, TaskCompletionSource<UserData> tcs)
    {
        // Call the updated LambdaAPI.GetUser coroutine.
        yield return LambdaAPI.GetUser(
            uid,
            onComplete: (JToken response) =>
            {
                try
                {
                    // Assuming UserData.FromJson now accepts a JToken.
                    UserData user = response.ToObject<UserData>();
                    AddUserCache(user);
                    tcs.SetResult(user);
                }
                catch (Exception ex)
                {
                    tcs.SetException(new Exception("Failed to parse user data: " + ex.Message));
                }
            },
            onError: (string error) =>
            {
                tcs.SetException(new Exception("Failed to fetch user from backend: " + error));
            }
        );
    }

    /// <summary>
    /// Adds a user to the cache if not already present.
    /// </summary>
    public static void AddUserCache(UserData user)
    {
        if (!users.Any(u => u.uid == user.uid))
        {
            users.Add(user);
        }
    }
}
