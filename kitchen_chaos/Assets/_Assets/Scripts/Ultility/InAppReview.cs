using UnityEngine;
// #if UNITY_ANDROID
// using UnityEngine.Android;
// #endif
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class InAppReview : MonoBehaviour
{
    private const string ReviewPromptKey = "HasReviewedApp";

    void Start()
    {
        // Check if the review prompt has already been shown
        if (!PlayerPrefs.HasKey(ReviewPromptKey))
        {
            ShowInAppReview();
        }
    }

    private void ShowInAppReview()
    {
        // #if UNITY_ANDROID
        //         ShowAndroidReview();
#if UNITY_IOS
        ShowIOSReview();
#endif
        // Mark that the review prompt has been shown
        PlayerPrefs.SetInt(ReviewPromptKey, 1);
        PlayerPrefs.Save();
    }

    // #if UNITY_ANDROID
    //     private void ShowAndroidReview()
    //     {
    //         AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    //         AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    //         AndroidJavaClass reviewManagerClass = new AndroidJavaClass("com.google.android.play.core.review.ReviewManagerFactory");
    //         AndroidJavaObject reviewManager = reviewManagerClass.CallStatic<AndroidJavaObject>("create", currentActivity);

    //         AndroidJavaObject reviewFlow = reviewManager.Call<AndroidJavaObject>("requestReviewFlow").Call<AndroidJavaObject>("get");

    //         reviewManager.Call("launchReviewFlow", currentActivity, reviewFlow);
    //     }
    // #endif

#if UNITY_IOS
    private void ShowIOSReview()
    {
        Device.RequestStoreReview();
    }
#endif
}
