using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class LambdaAPI : MonoBehaviour
{
    // Set your Lambda URL endpoint here (or retrieve it from your settings)
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

    private static void TestLog(string message)
    {
        Debug.Log("TEST: " + message);
    }

    #endregion

    #region HTTP Request Method

    /// <summary>
    /// Sends a POST request to the Lambda function URL with the specified payload.
    /// </summary>
    /// <param name="data">The JSON string to send in the request body.</param>
    /// <param name="callback">Callback invoked with the UnityWebRequest result.</param>
    public static IEnumerator BuildResponse1(string data, Action<UnityWebRequest> callback)
    {
        // Log the endpoint URL (after stripping any unwanted quotes)
        AttentionLog(serverUrlEndpoint);
        string url = serverUrlEndpoint.Replace("\"", "");

        // Convert the JSON string to UTF-8 encoded bytes
        byte[] bodyRaw = Encoding.UTF8.GetBytes(data);

        // Create a new UnityWebRequest configured for POST
        UnityWebRequest request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

        // Send the request and wait for it to complete
        yield return request.SendWebRequest();

        // (Optional) Log any network errors here if desired
#if UNITY_2020_1_OR_NEWER
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            ErrorLog("BuildResponse1 error: " + request.error);
        }

        // Return the request via callback
        callback(request);
    }

    #endregion

    #region Lambda Invocation

    // A simple serializable class to hold the payload
    // Note: Since "params" is a reserved word in C#, we use @params.
    [Serializable]
    public class LambdaPayload
    {
        public string func;
        public string @params;
    }

    /// <summary>
    /// Calls the Lambda function with the specified function name and parameters.
    /// </summary>
    /// <param name="func">Name of the function to invoke.</param>
    /// <param name="parameters">Parameters as a JSON string or simple string.</param>
    /// <param name="onComplete">Callback with the server response (as a string or JSON string) when successful.</param>
    /// <param name="onError">Callback with an error message if the invocation fails.</param>
    public static IEnumerator CallFuncWithParams1(string func, string parameters, Action<string> onComplete = null, Action<string> onError = null)
    {
        GreenLog("called: " + func);

        // Build the JSON payload.
        LambdaPayload payloadObj = new LambdaPayload
        {
            func = func,
            @params = parameters
        };
        string payload = JsonUtility.ToJson(payloadObj);

        GreenLog("request: " + func + " with: " + payload);

        UnityWebRequest responseRequest = null;
        bool done = false;

        // Send the POST request via BuildResponse1 and wait for the result.
        yield return BuildResponse1(payload, (request) =>
        {
            responseRequest = request;
            done = true;
        });

        // (Optional) Wait until done (should be immediate)
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

        // If the request succeeded and we received a 200 OK status...
        if (!isError && responseRequest.responseCode == 200)
        {
            // Decode the response text (UnityWebRequest automatically uses UTF-8)
            string responseString = responseRequest.downloadHandler.text;
            GreenLog("server response: " + responseString);

            try
            {
                // Attempt to parse JSON if needed.
                // (If you expect a specific response structure, you can define another serializable class
                //  and use JsonUtility.FromJson<T>(responseString) here.)
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
            // Handle non-200 status codes and network errors
            string errorMessage = "Lambda invocation " + func + " failed with status code " + responseRequest.responseCode + ".";
            ErrorLog(errorMessage);
            string errorResponse = string.Empty;

            if (!string.IsNullOrEmpty(responseRequest.downloadHandler.text))
            {
                try
                {
                    // If the error response is JSON, you could parse it here.
                    errorResponse = responseRequest.downloadHandler.text;
                    ErrorLog("Parsed error message: " + errorResponse);
                }
                catch (Exception e)
                {
                    errorResponse = responseRequest.downloadHandler.text;
                    ErrorLog("Non-JSON error response: " + errorResponse);
                }
            }
            onError?.Invoke(!string.IsNullOrEmpty(errorResponse) ? errorResponse : errorMessage);
        }
    }

    #endregion
}
