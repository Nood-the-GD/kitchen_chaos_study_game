using System;
using System.Collections;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Networking;

public class LambdaAPI : MonoBehaviour
{
    // Existing constant endpoint, logging helpers and BuildResponse1, CallFuncWithParams1 methods…
    public static string serverUrlEndpoint = Constant.SERVER_URL;

    #region Logging Helpers

    private static void AttentionLog(string message)
    {
        Debug.Log("<color=yellow>ATTENTION: " + message + "</color>");
    }

    private static void GreenLog(string message)
    {
        Debug.Log("<color=green>GREEN: " + message + "</color>");
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

    #endregion

    #region Lambda Invocation

    [Serializable]
    public class LambdaPayload
    {
        public string function;
        public string @params;
    }

    public static IEnumerator CallFuncWithParams1(string func, string parameters, Action<string> onComplete = null, Action<string> onError = null)
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

    #region Converted Lambda Functions

    // Assumes you have a UserData class with a CurrentUser property that has a uid field.
    // Also assumes SaveData.userToken, SocialData.GetChatSummaryFor(string), and AppController.CurrentRoomData exist.

    public static IEnumerator FindFriend(string searchName, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Build payload: { "uid": "...", "searchName": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"searchName\":\"" + searchName + "\"}";
        yield return CallFuncWithParams1("findFriend", payload, onComplete, onError);
    }

    public static IEnumerator SendFriendRequest(string otherUid, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\",\"otherUid\":\"" + otherUid + "\"}";
        yield return CallFuncWithParams1("sendFriendRequest", payload, onComplete, onError);
    }

    public static IEnumerator GetMySocial(Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid + "\"}";
        yield return CallFuncWithParams1("getMySocial", payload, onComplete, onError);
    }

    public static IEnumerator TryLogin(string uid, string token, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + uid + "\",\"token\":\"" + token + "\"}";
        yield return CallFuncWithParams1("login", payload, onComplete, onError);
    }

    public static IEnumerator TryLoginUsingAuth(string authToken, string gmail, string fuid, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "authToken": "...", "gmail": "...", "fuid": "..." }
        string payload = "{\"authToken\":\"" + authToken + "\",\"gmail\":\"" + gmail + "\",\"fuid\":\"" + fuid + "\"}";
        yield return CallFuncWithParams1("loginAuth", payload, onComplete, onError);
    }

    public static IEnumerator CreateUser(string userName, string gender, string email, string fuid, string authId, string quote, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "username": "...", "gender": "...", "email": "...", "authId": "...", "quote": "...", "fuid": "...", "timezone": "..." }
        string payload = "{\"username\":\"" + userName +
                         "\",\"gender\":\"" + gender +
                         "\",\"email\":\"" + email +
                         "\",\"authId\":\"" + authId +
                         "\",\"quote\":\"" + quote +
                         "\",\"fuid\":\"" + fuid +
                         "\",\"timezone\":\"" + GetCurrentTimeZoneOffset() + "\"}";
        yield return CallFuncWithParams1("createUser", payload, onComplete, onError);
    }

    public static IEnumerator AcceptFriend(string otherUid, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "token": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
                         
        yield return CallFuncWithParams1("acceptFriend", payload, onComplete, onError);
    }

    public static IEnumerator DeclineFriend(string otherUid, Action<string> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        yield return CallFuncWithParams1("declineFriend", payload, onComplete, onError);
    }

    public static IEnumerator DeleteFriend(string otherUid, Action<string> onComplete = null, Action<string> onError = null)
    {
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"token\":\"" + SaveData.userToken + "\"}";
        yield return CallFuncWithParams1("deleteFriend", payload, onComplete, onError);
    }

    public static IEnumerator LoadConvoData(string conversationId, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "token": "...", "convoId": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"token\":\"" + SaveData.userToken +
                         "\",\"convoId\":\"" + conversationId + "\"}";
        yield return CallFuncWithParams1("getConvoId", payload, onComplete, onError);
    }

    public static IEnumerator CreateChatMessage(string otherUid, string message, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "uid": "...", "otherUid": "...", "message": "..." }
        string payload = "{\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"otherUid\":\"" + otherUid +
                         "\",\"message\":\"" + message + "\"}";
        yield return CallFuncWithParams1("createChatMessage", payload, onComplete, onError);
    }

    public static IEnumerator SendChatMessage(string convoId, string message, Action<string> onComplete = null, Action<string> onError = null)
    {
        // Payload: { "convoId": "...", "uid": "...", "message": "..." }
        string payload = "{\"convoId\":\"" + convoId +
                         "\",\"uid\":\"" + UserData.currentUser.uid +
                         "\",\"message\":\"" + message + "\"}";
        yield return CallFuncWithParams1("syncChatMessage", payload, onComplete, onError);
    }

    // public static IEnumerator SendInviteRoom(string otherUid, Action<string> onComplete = null, Action<string> onError = null)
    // {
    //     // Build the invitation message from the current room data.
    //     // Assumes SocialData.GetChatSummaryFor(string) returns an object with an id field.
    //     var chatSummary = SocialData.GetChatSummaryFor(otherUid);
    //     string message = "{\"type\":\"inviteRoom\",\"roomId\":\"" + AppController.CurrentRoomData.id +
    //                      "\",\"roomMode\":\"" + AppController.CurrentRoomData.type + "\"}";

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
    public void hello(){
         Debug.Log("Hello World1");
        StartCoroutine(HelloWorld());
    }

    public IEnumerator HelloWorld(){
                string payload = "";
        Debug.Log("Hello World");
        yield return CallFuncWithParams1("helloWorld", payload, (response) => {
            Debug.Log(response);
        }, (error) => {
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
