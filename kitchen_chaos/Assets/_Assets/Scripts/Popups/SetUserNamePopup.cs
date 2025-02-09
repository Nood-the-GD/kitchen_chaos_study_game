using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;  // For Button and CanvasGroup

public class SetUserNamePopup : BasePopup<SetUserNamePopup>
{
    [HideInInspector]
    public bool isCreateUser = false;
    
    public GameObject closeButton;
    string userName;

    // --- Gender Selection ---
    public enum GenderType { Null, Male, Female }
    public GenderType selectedGender = GenderType.Null;

    // Assign these buttons in the Inspector (each should have a CanvasGroup component)
    public Button nullGenderButton;
    public Button maleGenderButton;
    public Button femaleGenderButton;
    // ------------------------

    public void Start(){
        SetCloseButton(!isCreateUser);

        // Ensure each gender button has a CanvasGroup component
        EnsureCanvasGroup(nullGenderButton);
        EnsureCanvasGroup(maleGenderButton);
        EnsureCanvasGroup(femaleGenderButton);

        // Set up listeners for each gender button
        if (nullGenderButton != null)
            nullGenderButton.onClick.AddListener(() => OnGenderButtonClicked(GenderType.Null));
        if (maleGenderButton != null)
            maleGenderButton.onClick.AddListener(() => OnGenderButtonClicked(GenderType.Male));
        if (femaleGenderButton != null)
            femaleGenderButton.onClick.AddListener(() => OnGenderButtonClicked(GenderType.Female));
    }

    public void SetName(string name){
        userName = name;
    }

    public void Next(){
        if(isCreateUser){
            Debug.Log("Creating User");
            StartCoroutine(LambdaAPI.CreateUser(userName, selectedGender.ToString(), (response) => {
                if(response != null){
                    //UserData.currentUser = response;
                    PhotonNetwork.NickName = userName;
                    HidePopup();
                }
            }));
        }
        else{
             Debug.Log("Updating User");
        }
    }

    public void SetCloseButton(bool isShow){
        closeButton.SetActive(isShow);
    }

    // --- Gender Button Handling ---
    private void OnGenderButtonClicked(GenderType gender)
    {
        selectedGender = gender;
        AnimateGenderButtons(gender);
    }

    /// <summary>
    /// Animates each gender button so that the selected button fades to full opacity (1) with a pop effect,
    /// while the other buttons fade to an opacity of 0.5.
    /// </summary>
    /// <param name="selected">The gender type that was selected.</param>
    private void AnimateGenderButtons(GenderType selected)
    {
        float fadeDuration = 0.3f;

        // Get CanvasGroup components from each button
        CanvasGroup cgNull = nullGenderButton.GetComponent<CanvasGroup>();
        CanvasGroup cgMale = maleGenderButton.GetComponent<CanvasGroup>();
        CanvasGroup cgFemale = femaleGenderButton.GetComponent<CanvasGroup>();

        // For the selected button, fade to alpha 1 and trigger a pop animation.
        // For the others, fade to alpha 0.5.
        if (selected == GenderType.Null)
        {
            StartCoroutine(FadeCanvasGroup(cgNull, cgNull.alpha, 1, fadeDuration));
            StartCoroutine(PopAnimation(nullGenderButton.transform));
            StartCoroutine(FadeCanvasGroup(cgMale, cgMale.alpha, 0.2f, fadeDuration));
            StartCoroutine(FadeCanvasGroup(cgFemale, cgFemale.alpha, 0.2f, fadeDuration));
        }
        else if (selected == GenderType.Male)
        {
            StartCoroutine(FadeCanvasGroup(cgNull, cgNull.alpha, 0.2f, fadeDuration));
            StartCoroutine(FadeCanvasGroup(cgMale, cgMale.alpha, 1, fadeDuration));
            StartCoroutine(PopAnimation(maleGenderButton.transform));
            StartCoroutine(FadeCanvasGroup(cgFemale, cgFemale.alpha, 0.2f, fadeDuration));
        }
        else if (selected == GenderType.Female)
        {
            StartCoroutine(FadeCanvasGroup(cgNull, cgNull.alpha, 0.2f, fadeDuration));
            StartCoroutine(FadeCanvasGroup(cgMale, cgMale.alpha, 0.2f, fadeDuration));
            StartCoroutine(FadeCanvasGroup(cgFemale, cgFemale.alpha, 1, fadeDuration));
            StartCoroutine(PopAnimation(femaleGenderButton.transform));
        }
    }

    /// <summary>
    /// Smoothly fades a CanvasGroup's alpha from the starting value to the ending value over the specified duration.
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = endAlpha;
    }

    /// <summary>
    /// Performs a pop animation by briefly scaling the button up and then returning it to its original scale.
    /// </summary>
    private IEnumerator PopAnimation(Transform t)
    {
        Vector3 originalScale = t.localScale;
        Vector3 targetScale = originalScale * 1.2f; // Increase scale by 20%
        float popDuration = 0.1f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < popDuration)
        {
            t.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / popDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        t.localScale = targetScale;

        // Scale back to original
        elapsed = 0f;
        while (elapsed < popDuration)
        {
            t.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / popDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        t.localScale = originalScale;
    }

    /// <summary>
    /// Ensures that the provided button has a CanvasGroup component to control its opacity.
    /// </summary>
    private void EnsureCanvasGroup(Button btn)
    {
        if (btn != null && btn.GetComponent<CanvasGroup>() == null)
        {
            btn.gameObject.AddComponent<CanvasGroup>();
        }
    }
}
