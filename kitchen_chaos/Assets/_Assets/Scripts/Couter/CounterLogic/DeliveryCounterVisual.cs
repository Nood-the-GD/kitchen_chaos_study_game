using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class DeliveryCounterVisual : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private MMF_Player messageFeedback;

    public void ShowMessage(string message)
    {
        messageFeedback.StopFeedbacks();
        messageText.text = message;
        messageFeedback.PlayFeedbacks();
    }
}
