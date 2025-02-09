using System;
using DG.DemiEditor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlideNotificationPopup : BasePopup<SlideNotificationPopup>
{
    public Sprite errorSprite;
    public Sprite notiSprite;
    public Image notiImage;
    public Text title;
    public Text message;

    // Animation settings (adjust as needed)
    public float animationDuration = 0.5f;
    public float displayDuration = 4f;
    public float targetWidth = 450f;

    /// <summary>
    /// Displays the message with a slide animation using DOTween.
    /// </summary>
    public void ShowMessage(string message, bool isError, string title = "" )
    {

        if(title.IsNullOrEmpty()){
            if(isError){
                title = "Error";
            }
            else{
                title = "Notification";
            }
        }
        // Set texts and sprite.
        this.title.text = title;
        this.message.text = message;
        notiImage.sprite = isError ? errorSprite : notiSprite;

        
        // Get the RectTransform and set its initial width to 0.
        RectTransform rt = notiImage.GetComponent<RectTransform>();
        Vector2 initialSize = rt.sizeDelta;
        rt.sizeDelta = new Vector2(0, initialSize.y);

        // Create a DOTween sequence to animate the width.
        Sequence sequence = DOTween.Sequence();

        // Animate width from 0 to targetWidth.
        sequence.Append(
            DOTween.To(
                () => rt.sizeDelta.x,
                x =>
                {
                    Vector2 newSize = rt.sizeDelta;
                    newSize.x = x;
                    rt.sizeDelta = newSize;
                },
                targetWidth,
                animationDuration
            )
        );

        // Wait for the display duration.
        sequence.AppendInterval(displayDuration);

        // Animate width back from targetWidth to 0.
        sequence.Append(
            DOTween.To(
                () => rt.sizeDelta.x,
                x =>
                {
                    Vector2 newSize = rt.sizeDelta;
                    newSize.x = x;
                    rt.sizeDelta = newSize;
                },
                0,
                animationDuration
            )
        );

        // When the animation is complete, call doneShow.
        sequence.OnComplete(DoneShow);
    }

    /// <summary>
    /// Called when the animation is complete.
    /// </summary>
    void DoneShow()
    {
        HidePopup();
    }
}
