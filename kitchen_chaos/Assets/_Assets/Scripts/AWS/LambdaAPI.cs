using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;
using CandyCoded.env;
using Newtonsoft.Json.Linq;

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
        Debug.Log("<color=green>GREEN: " + "</color>" +message );
    }

    private static void ErrorLog(string message)
    {
        Debug.LogError("<color=red>ERROR: " + message + "</color>");
    }

    #endregion

    #region HTTP Request Method

    public static IEnumerator BuildResponse1(string data, Action<UnityWebRequest> callback)
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

        yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            ErrorLog("BuildResponse1 error: " + request.error);
        }
        
        callback(request);
    }

    public static IEnumerator CallFuncWithURL(string func, string parameters, Action<string> onComplete = null, Action<string> onError = null)
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

        UnityWebRequest responseRequest = null;
        bool done = false;
        yield return BuildResponse1(payload, (request) =>
        {
            responseRequest = request;
            done = true;
        });

        while (!done)
            yield return null;

        if (responseRequest == null)
        {
            ErrorLog("No response received.");
            onError?.Invoke("No response received.");
            yield break;
        }

#if UNITY_2020_1_OR_NEWER
        bool isError = responseRequest.result == UnityWebRequest.Result.ConnectionError || responseRequest.result == UnityWebRequest.Result.ProtocolError;
#else
        bool isError = responseRequest.isNetworkError || responseRequest.isHttpError;
#endif

        if (!isError && responseRequest.responseCode == 200)
        {
            string responseString = responseRequest.downloadHandler.text;
            GreenLog("server response: " + responseString);
            try
            {
                onComplete?.Invoke(responseString);
            }
            catch (Exception e)
            {
                GreenLog("JSON parsing error: " + e.Message + ". Returning raw response.");
                onComplete?.Invoke(responseString);
            }
        }
        else
        {
            string errorMessage = "Lambda invocation " + func + " failed with status code " + responseRequest.responseCode + ".";
            ErrorLog(errorMessage);
            string errorResponse = (!string.IsNullOrEmpty(responseRequest.downloadHandler.text)) ? responseRequest.downloadHandler.text : errorMessage;
            onError?.Invoke(errorResponse);
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

    #region AWS Lambda SDK Invocation

    // These methods use the AWS SDK for .NET to call your Lambda function directly.
    // (Be sure to securely store or retrieve your AWS credentials in a production app.)

    /// <summary>
    /// Returns an AmazonLambdaClient configured for the ap-southeast-1 region.
    /// </summary>
    /// <returns>AmazonLambdaClient instance.</returns>
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
    /// Invokes the 'playbond-connections' Lambda function with the specified payload.
    /// </summary>
    /// <param name="service">AmazonLambdaClient instance.</param>
    /// <param name="data">Payload data as string.</param>
    /// <returns>InvokeResponse from AWS Lambda.</returns>
    public static async Task<InvokeResponse> BuildResponse2(AmazonLambdaClient service, string data)
    {
        var request = new InvokeRequest
        {
            // The AWS Lambda function name is hardcoded here.
            // You can change this if you need to dynamically select the function.
            FunctionName = "kitchen-main",
            InvocationType = InvocationType.RequestResponse, // Wait for the response
            LogType = Amazon.Lambda.LogType.Tail, // Change to 'None' if you do not need logs
            Payload = data
        };

        return await service.InvokeAsync(request);
    }

    /// <summary>
    /// AWS Lambda invocation using the AWS SDK.
    /// This version accepts a function name and parameters, builds a JSON payload,
    /// and then invokes the Lambda function.
    /// </summary>
    /// <param name="func">The function name to call (as defined in your Lambda payload).</param>
    /// <param name="parameters">The parameters to pass.</param>
    /// <param name="onComplete">Callback with the response string on success.</param>
    /// <param name="onError">Callback with error message on failure.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    public static IEnumerator CallAwsLambda(string func, string parameters, Action<string> onComplete = null, Action<string> onError = null)
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
        Task<InvokeResponse> task = BuildResponse2(service, payload);

        // Wait until the asynchronous task is done.
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            string errorMsg = task.Exception.Message;
            ErrorLog("AWS Lambda Invocation error: " + errorMsg);
            ErrorLog("Full error: " + task.Exception.ToString());
            
            onError?.Invoke(errorMsg);
        }
        else
        {
            InvokeResponse response = task.Result;
            string resultString = string.Empty;
            using (StreamReader sr = new StreamReader(response.Payload))
            {
                resultString = sr.ReadToEnd();
            }
            GreenLog("AWS Lambda response: " + resultString);
            onComplete?.Invoke(resultString);
        }
    }

    #endregion

    #endregion

    #region Combined Lambda Call


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

    static void Notification(string error){
        Debug.LogError(error);
        var p = SlideNotificationPopup.ShowPopup();
        p.ShowMessage(error, true);
    }

    /// <summary>
    /// Base method to call a Lambda function.
    /// If <paramref name="useAws"/> is false, it will use the HTTP method (CallFuncWithURL),
    /// and if true, it will call the AWS Lambda via the AWS SDK (CallAwsLambda).
    /// </summary>
    /// <param name="func">The function name to call.</param>
    /// <param name="parameters">The parameters for the function call.</param>
    /// <param name="onComplete">Callback with the response JObject on success.</param>
    /// <param name="onError">Callback with error message on failure.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    public static IEnumerator CallLambdaBase(
        string func, 
        string parameters, 
        Action<JToken> onComplete = null, 
        Action<string> onError = null)
    {
        // Validate input.
        if (string.IsNullOrEmpty(func))
        {
            string errorMsg = "Function name is required.";
            Notification(errorMsg);
            onError?.Invoke(errorMsg);
            yield break;
        }

        // Determine which call method to use based on platform or editor settings.
        bool useAws = false;
        if (Application.isEditor)
        {
            // Try to read an environment variable (if available) to determine lambda mode.
            if (env.TryParseEnvironmentVariable("lambdaMode", out int mode))
            {
                useAws = (mode == 1);
            }
        }
        else if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // For mobile platforms, always use the HTTP method.
            useAws = false;
        }

        Debug.Log($"useAws: {useAws} | Calling: {func} with parameters: {parameters}");

        // Define a common response handler.
        Action<string> responseHandler = data =>
        {
            JObject json = StringToJObject(data);
            if (json == null)
            {
                string errorMsg = "Error parsing JSON response.";
                Notification(errorMsg);
                onError?.Invoke(errorMsg);
                return;
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
                    onComplete?.Invoke(json["body"]);
                }
                else
                {
                    Notification(body);
                    onError?.Invoke(body);
                }
            }
            else
            {
                // Non-AWS response is assumed to have a different structure.
                // Simply pass the parsed JSON without attempting to access "body".
                onComplete?.Invoke(json);
            }
        };

        // Call the appropriate method.
        if (useAws)
        {
            yield return CallAwsLambda(func, parameters, responseHandler, error => { 
                Notification(error);
                onError?.Invoke(error);
            });
        }
        else
        {
            yield return CallFuncWithURL(func, parameters, responseHandler, error => { 
                Notification(error);
                onError?.Invoke(error);
            });
        }
    }



    #endregion

    #region Converted Lambda Functions

    // Assumes you have a UserData class with a currentUser property that has a uid field.
    // Also assumes SaveData.userToken, SocialData.GetChatSummaryFor(string), and AppController.CurrentRoomData exist.

    public static IEnumerator FindFriend(string searchName, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Build payload: { "uid": "...", "searchName": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"searchName\":\"" + searchName + "\"}";
        yield return CallLambdaBase("findFriend", payload, onComplete, onError);
    }

    public static IEnumerator SendFriendRequest(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"otherUid\":\"" + otherUid + "\"}";
        yield return CallLambdaBase("sendFriendRequest", payload, onComplete, onError);
    }

    public static IEnumerator GetMySocial(Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\"}";
        yield return CallLambdaBase("getMySocial", payload, onComplete, onError);
    }

    public static IEnumerator TryLogin(string uid, string token, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + uid + "\",\"token\":\"" + token + "\"}";
        yield return CallLambdaBase("login", payload, onComplete, onError);
    }

    public static IEnumerator TryLoginUsingAuth(string authToken, string gmail, string fuid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "authToken": "...", "gmail": "...", "fuid": "..." }
        string payload = "{\"authToken\":\"" + authToken + "\",\"gmail\":\"" + gmail + "\",\"fuid\":\"" + fuid + "\"}";
        yield return CallLambdaBase("loginAuth", payload, onComplete, onError);
    }

    public static IEnumerator CreateUser(string userName, string gender, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "username": "...", "gender": "...", "email": "...", "authId": "...", "quote": "...", "fuid": "...", "timezone": "..." }
        string payload = "{\"username\":\"" + userName +
                         "\",\"gender\":\"" + gender +
                         "\",\"timezone\":\"" + GetCurrentTimeZoneOffset() + "\"}";
        yield return CallLambdaBase("createUser", payload, onComplete, onError);
    }

    public static IEnumerator GetUser(string otherUid,Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        yield return CallLambdaBase("getUser", payload, onComplete, onError);
    }


    public static IEnumerator AcceptFriend(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        yield return CallLambdaBase("acceptFriend", payload, onComplete, onError);
    }

    


    public static IEnumerator DeclineFriend(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        yield return CallLambdaBase("declineFriend", payload, onComplete, onError);
    }

    public static IEnumerator DeleteFriend(string otherUid, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        yield return CallLambdaBase("deleteFriend", payload, onComplete, onError);
    }

    public static IEnumerator LoadConvoData(string conversationId, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "token": "...", "convoId": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"token\":\"" + SaveData.userToken +
                         "\",\"convoId\":\"" + conversationId + "\"}";
        yield return CallLambdaBase("getConvoId", payload, onComplete, onError);
    }

    public static IEnumerator CreateChatMessage(string otherUid, string message, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "message": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"message\":\"" + message + "\"}";
        yield return CallLambdaBase("createChatMessage", payload, onComplete, onError);
    }

    public static IEnumerator SendChatMessage(string convoId, string message, Action<JToken> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "convoId": "...", "uid": "...", "message": "..." }
        string payload = "{\"convoId\":\"" + convoId +
                         "\",\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"message\":\"" + message + "\"}";
        yield return CallLambdaBase("syncChatMessage", payload, onComplete, onError);
    }

    // Uncomment and modify as needed.
    // public static IEnumerator SendInviteRoom(string otherUid, Action<string> onComplete = null, Action<string> onError = null)
    // {
    //     // Build the invitation message from the current room data.
    //     // Assumes SocialData.GetChatSummaryFor(string) returns an object with an id field.
    //     var chatSummary = SocialData.GetChatSummaryFor(otherUid);
    //     string message = "{\"type\":\"inviteRoom\",\"roomId\":\"" + AppController.CurrentRoomData.id +
    //                      "\",\"roomMode\":\"" + AppController.CurrentRoomData.type + "\"}";
    //
    //     if (chatSummary == null)
    //     {
    //         // If no chat summary exists, create a new chat message.
    //         yield return CreateChatMessage(otherUid, message, onComplete, onError);
    //     }
    //     else
    //     {
    //         // Otherwise, send the message to the existing conversation.
    //         yield return SendChatMessage(chatSummary.id, message, onComplete, onError);
    //     }
    // }

    [Button]
    public void hello()
    {
        Debug.Log("Hello World1");
        StartCoroutine(HelloWorld());
    }

    public IEnumerator HelloWorld()
    {
        string payload = "";
        Debug.Log("Hello World");
        yield return CallLambdaBase("helloWorld", payload,(response) =>
        {
            Debug.Log(response);
        }, (error) =>
        {
            Debug.LogError(error);
        });
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Returns the current timezone offset as a string.
    /// </summary>
    /// <returns>A string representing the timezone offset (for example, "+02:00").</returns>
    private static string GetCurrentTimeZoneOffset()
    {
        TimeSpan offset = DateTimeOffset.Now.Offset;
        // Format the offset as a string, e.g. "+02:00" or "-05:00"
        return (offset < TimeSpan.Zero ? "-" : "+") + offset.ToString(@"hh\:mm");
    }

    #endregion
}
