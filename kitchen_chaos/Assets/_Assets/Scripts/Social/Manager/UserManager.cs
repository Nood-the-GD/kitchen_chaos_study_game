using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json.Linq;

public static class UserManager
{
    // In-memory cache of users.
    private static List<UserData> users = new List<UserData>();

    // Dictionary to track ongoing fetch requests.
    // Key: uid, Value: UniTaskCompletionSource that will eventually complete with a UserData.
    private static Dictionary<string, UniTaskCompletionSource<UserData>> _pendingRequests = new Dictionary<string, UniTaskCompletionSource<UserData>>();

    // Maximum number of concurrent pending requests.
    private const int _maxPendingRequests = 10;

    // Delay duration if max pending is reached.
    private static readonly TimeSpan _pendingWaitDuration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Attempts to return the cached user for the specified uid.
    /// If not found, fetches from the backend.
    /// </summary>
    public static async UniTask<UserData> GetUser(string uid)
    {
        // Try to find the user in the cache.
        UserData existingUser = users.FirstOrDefault(user => user.uid == uid);
        if (existingUser != null)
        {
            return existingUser;
        }

        // Check if there's an ongoing fetch for this uid.
        if (_pendingRequests.TryGetValue(uid, out UniTaskCompletionSource<UserData> pendingSource))
        {
            return await pendingSource.Task;
        }

        // Wait if too many pending requests are active.
        while (_pendingRequests.Count >= _maxPendingRequests)
        {
            await UniTask.Delay(_pendingWaitDuration);
        }

        // Create a new completion source for this request
        var completionSource = new UniTaskCompletionSource<UserData>();
        _pendingRequests[uid] = completionSource;

        try
        {
            // Start a new fetch request.
            UserData user = await FetchUserFromBackend(uid);
            
            // Complete the task with the result
            completionSource.TrySetResult(user);
            
            return user;
        }
        catch (Exception ex)
        {
            // Set the exception on the completion source
            completionSource.TrySetException(ex);
            throw;
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
    /// This version uses UniTask instead of coroutines.
    /// </summary>
    private static async UniTask<UserData> FetchUserFromBackend(string uid)
    {
        // Call LambdaAPI.GetUser, which returns a ServerRespone containing the user data.
        ServerRespone response = await LambdaAPI.GetUser(uid);
        if (response.IsError)
        {
            throw new Exception("Failed to fetch user from backend: " + response.error);
        }

        try
        {
            // Convert the JToken to a UserData instance.
            UserData user = response.jToken.ToObject<UserData>();
            AddUserCache(user);
            return user;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to parse user data: " + ex.Message);
        }
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
