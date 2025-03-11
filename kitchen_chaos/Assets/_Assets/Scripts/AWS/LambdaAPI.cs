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

public class ServerRespone
{
    public JToken jToken;
    public string error;

    public bool IsError => !string.IsNullOrEmpty(error);
    public bool IsSuccess => !IsError;
}

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
        //AttentionLog(serverUrlEndpoint);
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

    static JToken StringToJObject(string data)
    {
        try
        {
            return JToken.Parse(data);
        }
        catch (Exception e)
        {
            // Optionally log the error.
            return null;
        }
    }

    static void Notification(string error)
    {

        var p = SlideNotificationPopup.ShowPopup();
        p.ShowMessage(error, true);
    }

    /// <summary>
    /// Base method to call a Lambda function.
    /// Determines whether to use the HTTP method or the AWS SDK.
    /// </summary>
    public static async UniTask<JToken> CallLambdaBaseAsync(string func, string parameters, bool showNoti = false)
    {
        if (string.IsNullOrEmpty(func))
        {
            string errorMsg = "Function name is required.";
            Debug.LogError(errorMsg);
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
            Debug.LogError(ex.Message);
            if (showNoti)
            {

                Notification(ex.Message);
            }
            throw;
        }

        var json = StringToJObject(data);
        if (json == null)
        {
            Debug.Log("Cannot parse the JSON");
            return null;
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
                Debug.LogError(body);
                if (showNoti)
                {

                    Notification(body);
                }
                throw new Exception(body);
            }
        }
        else
        {
            return json;
        }
    }

    #endregion

    #region Lambda Functions Returning ServerRespone

    /// <summary>
    /// Finds a friend using the provided search name.
    /// </summary>
    public static async UniTask<ServerRespone> FindFriend(string searchName)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"searchName\":\"" + searchName + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("findFriend", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Sends a friend request.
    /// </summary>
    public static async UniTask<ServerRespone> SendFriendRequest(string otherUid)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"otherUid\":\"" + otherUid + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("sendFriendRequest", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Retrieves social data.
    /// </summary>
    public static async UniTask<ServerRespone> GetMySocial()
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("getMySocial", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Tries logging in with uid and token.
    /// </summary>
    public static async UniTask<ServerRespone> TryLogin(string uid, string token)
    {
        string payload = "{\"uid\":\"" + uid + "\",\"token\":\"" + token + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("login", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Tries logging in using authentication details.
    /// </summary>
    public static async UniTask<ServerRespone> TryLoginUsingAuth(string authToken, string gmail, string fuid)
    {
        string payload = "{\"authToken\":\"" + authToken + "\",\"gmail\":\"" + gmail + "\",\"fuid\":\"" + fuid + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("loginAuth", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public static async UniTask<ServerRespone> CreateUser(string userName, string gender)
    {
        string payload = "{\"username\":\"" + userName +
                         "\",\"gender\":\"" + gender +
                         "\",\"timezone\":\"" + GetCurrentTimeZoneOffset() + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("createUser", payload, showNoti: true);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Retrieves data for a specific user.
    /// </summary>
    public static async UniTask<ServerRespone> GetUser(string otherUid)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("getUser", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Accepts a friend request.
    /// </summary>
    public static async UniTask<ServerRespone> AcceptFriend(string otherUid)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("acceptFriend", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    public static async UniTask<ServerRespone> AcceptFriendCustom(string fromUid, string otherUid)
    {
        string payload = "{\"uid\":\"" + fromUid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("acceptFriend", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Declines a friend request.
    /// </summary>
    public static async UniTask<ServerRespone> DeclineFriend(string otherUid)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("declineFriend", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Deletes a friend.
    /// </summary>
    public static async UniTask<ServerRespone> DeleteFriend(string otherUid)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("deleteFriend", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Loads conversation data.
    /// </summary>
    public static async UniTask<ServerRespone> LoadConvoData(string conversationId)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"token\":\"" + SaveData.userToken +
                         "\",\"convoId\":\"" + conversationId + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("getConvoId", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Creates a chat message.
    /// </summary>
    public static async UniTask<ServerRespone> CreateChatMessage(string otherUid, string message)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"message\":\"" + message + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("createChatMessage", payload, showNoti: true);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    /// <summary>
    /// Sends a chat message.
    /// </summary>
    public static async UniTask<ServerRespone> SendChatMessage(string convoId, string message)
    {
        string payload = "{\"convoId\":\"" + convoId +
                         "\",\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"message\":\"" + message + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("syncChatMessage", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    [Button]
    public void Hello()
    {
        Debug.Log("Hello World1");
        // Example call for testing the HelloWorld function.
        HelloWorld().ContinueWith(response =>
        {
            if (response.IsError)
            {
                Debug.LogError("HelloWorld error: " + response.error);
            }
            else
            {
                Debug.Log("HelloWorld response: " + response.jToken);
            }
        });
    }

    public static async UniTask<ServerRespone> HelloWorld()
    {
        string payload = "";
        Debug.Log("Hello World");
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("helloWorld", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
    }

    public static async UniTask<ServerRespone> TestStream()
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\"}";
        ServerRespone response = new ServerRespone();
        try
        {
            JToken result = await CallLambdaBaseAsync("testStream", payload);
            response.jToken = result;
        }
        catch (Exception ex)
        {
            response.error = ex.Message;
        }
        return response;
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
