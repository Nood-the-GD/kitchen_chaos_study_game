using System;
using System.IO;
using System.Text;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using CandyCoded.env;
using Newtonsoft.Json.Linq;
using Cysharp.Threading.Tasks;

public class LambdaAPI : MonoBehaviour
{
    // Existing constant endpoint.
    public static string serverUrlEndpoint = Constant.SERVER_URL;

    #region Logging Helpers

    private static void AttentionLog(string message)
    {
        Debug.Log("<color=yellow>ATTENTION: " + message + "</color>");
    }

    private static void GreenLog(string message)
    {
        Debug.Log("<color=green>GREEN: </color>" + message);
    }

    private static void ErrorLog(string message)
    {
        Debug.LogError("<color=red>ERROR: " + message + "</color>");
    }

    #endregion

    #region HTTP Request Methods (UniTask)

    /// <summary>
    /// Sends a HTTP POST request with the provided data.
    /// </summary>
    public static async UniTask<UnityWebRequest> BuildResponse1Async(string data)
    {
        AttentionLog(serverUrlEndpoint);
        string url = serverUrlEndpoint.Replace("\"", "");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);

        UnityWebRequest request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

        await request.SendWebRequest().ToUniTask();

#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            ErrorLog("BuildResponse1 error: " + request.error);
        }

        return request;
    }

    /// <summary>
    /// Calls the Lambda function using the HTTP URL endpoint.
    /// </summary>
    public static async UniTask<string> CallFuncWithURLAsync(string func, string parameters)
    {
        GreenLog("called: " + func);

        // Build the JSON payload using our LambdaPayload helper.
        LambdaPayload payloadObj = new LambdaPayload
        {
            function = func,
            @params = parameters
        };
        string payload = JsonUtility.ToJson(payloadObj);
        GreenLog("request: " + func + " with: " + payload);

        UnityWebRequest responseRequest = await BuildResponse1Async(payload);

#if UNITY_2020_1_OR_NEWER
        bool isError = responseRequest.result == UnityWebRequest.Result.ConnectionError || responseRequest.result == UnityWebRequest.Result.ProtocolError;
#else
        bool isError = responseRequest.isNetworkError || responseRequest.isHttpError;
#endif

        if (!isError && responseRequest.responseCode == 200)
        {
            string responseString = responseRequest.downloadHandler.text;
            GreenLog("server response: " + responseString);
            return responseString;
        }
        else
        {
            string errorMessage = "Lambda invocation " + func + " failed with status code " + responseRequest.responseCode + ".";
            ErrorLog(errorMessage);
            string errorResponse = (!string.IsNullOrEmpty(responseRequest.downloadHandler.text))
                ? responseRequest.downloadHandler.text
                : errorMessage;
            throw new Exception(errorResponse);
        }
    }

    #endregion

    #region Lambda Invocation

    [Serializable]
    public class LambdaPayload
    {
        public string function;
        public string @params;
    }

    #region AWS Lambda SDK Invocation (UniTask)

    /// <summary>
    /// Returns an AmazonLambdaClient configured for the ap-southeast-1 region.
    /// </summary>
    public static AmazonLambdaClient GetAwsLambdaService()
    {
        var awsAccessKey = "awsAccesskey";
        var awsSecretKey = "awsSecretKey";
        if (env.TryParseEnvironmentVariable("awsAccesskey", out string key))
        {
            awsAccessKey = key;
        }
        if (env.TryParseEnvironmentVariable("awsSecretKey", out string secret))
        {
            awsSecretKey = secret;
        }

        var credentials = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
        var config = new AmazonLambdaConfig { RegionEndpoint = RegionEndpoint.APSoutheast1 };
        return new AmazonLambdaClient(credentials, config);
    }

    /// <summary>
    /// Invokes the 'kitchen-main' Lambda function with the specified payload.
    /// </summary>
    public static async UniTask<InvokeResponse> BuildResponse2Async(AmazonLambdaClient service, string data)
    {
        var request = new InvokeRequest
        {
            FunctionName = "kitchen-main",
            InvocationType = InvocationType.RequestResponse, // Wait for the response
            LogType = Amazon.Lambda.LogType.Tail,             // Change to 'None' if you do not need logs
            Payload = data
        };

        return await service.InvokeAsync(request);
    }

    /// <summary>
    /// AWS Lambda invocation using the AWS SDK.
    /// </summary>
    public static async UniTask<string> CallAwsLambdaAsync(string func, string parameters)
    {
        // Build the JSON payload using our LambdaPayload helper.
        LambdaPayload payloadObj = new LambdaPayload
        {
            function = func,
            @params = parameters
        };
        string payload = JsonUtility.ToJson(payloadObj);
        GreenLog("AWS request: " + func + " with: " + payload);

        AmazonLambdaClient service = GetAwsLambdaService();
        InvokeResponse response;
        try
        {
            response = await BuildResponse2Async(service, payload);
        }
        catch (Exception ex)
        {
            ErrorLog("AWS Lambda Invocation error: " + ex.Message);
            ErrorLog("Full error: " + ex.ToString());
            throw;
        }

        string resultString = string.Empty;
        using (StreamReader sr = new StreamReader(response.Payload))
        {
            resultString = sr.ReadToEnd();
        }
        GreenLog("AWS Lambda response: " + resultString);
        return resultString;
    }

    #endregion

    #endregion

    #region Combined Lambda Call (UniTask)

    static JObject StringToJObject(string data)
    {
        try
        {
            return JObject.Parse(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing JSON: " + e.Message);
            return null;
        }
    }

    static void Notification(string error)
    {
        Debug.LogError(error);
        var p = SlideNotificationPopup.ShowPopup();
        p.ShowMessage(error, true);
    }

    /// <summary>
    /// Base method to call a Lambda function.
    /// Determines whether to use the HTTP method or the AWS SDK.
    /// </summary>
    public static async UniTask<JToken> CallLambdaBaseAsync(string func, string parameters)
    {
        if (string.IsNullOrEmpty(func))
        {
            string errorMsg = "Function name is required.";
            Notification(errorMsg);
            throw new Exception(errorMsg);
        }

        bool useAws = false;
        if (Application.isEditor)
        {
            if (env.TryParseEnvironmentVariable("lambdaMode", out int mode))
            {
                useAws = (mode == 1);
            }
        }
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            useAws = false;
        }

        Debug.Log($"useAws: {useAws} | Calling: {func} with parameters: {parameters}");

        string data;
        try
        {
            if (useAws)
            {
                data = await CallAwsLambdaAsync(func, parameters);
            }
            else
            {
                data = await CallFuncWithURLAsync(func, parameters);
            }
        }
        catch (Exception ex)
        {
            Notification(ex.Message);
            throw;
        }

        JObject json = StringToJObject(data);
        if (json == null)
        {
            string errorMsg = "Error parsing JSON response.";
            Notification(errorMsg);
            throw new Exception(errorMsg);
        }

        if (useAws)
        {
            // AWS response is expected to include "statusCode" and "body".
            int statusCode = json["statusCode"]?.Value<int>() ?? 0;
            string body = json["body"]?.Type == JTokenType.String
                ? json["body"].Value<string>()
                : json["body"]?.ToString() ?? "No message provided";

            if (statusCode == 200)
            {
                return json["body"];
            }
            else
            {
                Notification(body);
                throw new Exception(body);
            }
        }
        else
        {
            return json;
        }
    }

    #endregion

    #region Converted Lambda Functions (with onComplete and onError callbacks)

    /// <summary>
    /// Finds a friend using the provided search name.
    /// </summary>
    public static async UniTask FindFriend(string searchName, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"searchName\":\"" + searchName + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("findFriend", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Sends a friend request.
    /// </summary>
    public static async UniTask SendFriendRequest(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"otherUid\":\"" + otherUid + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("sendFriendRequest", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves social data.
    /// </summary>
    public static async UniTask GetMySocial(Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("getMySocial", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Tries logging in with uid and token.
    /// </summary>
    public static async UniTask TryLogin(string uid, string token, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + uid + "\",\"token\":\"" + token + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("login", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Tries logging in using authentication details.
    /// </summary>
    public static async UniTask TryLoginUsingAuth(string authToken, string gmail, string fuid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"authToken\":\"" + authToken + "\",\"gmail\":\"" + gmail + "\",\"fuid\":\"" + fuid + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("loginAuth", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new user.
    /// (Renamed from CreateUserAsync to CreateUser.)
    /// </summary>
    public static async UniTask CreateUser(string userName, string gender, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"username\":\"" + userName +
                         "\",\"gender\":\"" + gender +
                         "\",\"timezone\":\"" + GetCurrentTimeZoneOffset() + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("createUser", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Retrieves data for a specific user.
    /// </summary>
    public static async UniTask GetUser(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("getUser", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Accepts a friend request.
    /// </summary>
    public static async UniTask AcceptFriend(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("acceptFriend", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Declines a friend request.
    /// </summary>
    public static async UniTask DeclineFriend(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("declineFriend", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Deletes a friend.
    /// </summary>
    public static async UniTask DeleteFriend(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("deleteFriend", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Loads conversation data.
    /// </summary>
    public static async UniTask LoadConvoData(string conversationId, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"token\":\"" + SaveData.userToken +
                         "\",\"convoId\":\"" + conversationId + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("getConvoId", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Creates a chat message.
    /// </summary>
    public static async UniTask CreateChatMessage(string otherUid, string message, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"message\":\"" + message + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("createChatMessage", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// Sends a chat message.
    /// </summary>
    public static async UniTask SendChatMessage(string convoId, string message, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"convoId\":\"" + convoId +
                         "\",\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"message\":\"" + message + "\"}";
        try
        {
            JToken result = await CallLambdaBaseAsync("syncChatMessage", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    [Button]
    public void Hello()
    {
        Debug.Log("Hello World1");
        // Example call using the callback pattern.
        HelloWorld(
            onComplete: (result) => Debug.Log("HelloWorld response: " + result),
            onError: (err) => Debug.LogError("HelloWorld error: " + err)
        ).Forget();
    }

    public static async UniTask HelloWorld(Action<JToken> onComplete = null, Action<string> onError = null)
    {
        string payload = "";
        Debug.Log("Hello World");
        try
        {
            JToken result = await CallLambdaBaseAsync("helloWorld", payload);
            onComplete?.Invoke(result);
        }
        catch (Exception ex)
        {
            onError?.Invoke(ex.Message);
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Returns the current timezone offset as a string (e.g., "+02:00" or "-05:00").
    /// </summary>
    private static string GetCurrentTimeZoneOffset()
    {
        TimeSpan offset = DateTimeOffset.Now.Offset;
        return (offset < TimeSpan.Zero ? "-" : "+") + offset.ToString(@"hh\:mm");
    }

    #endregion
}
