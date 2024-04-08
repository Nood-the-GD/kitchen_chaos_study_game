using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

public class PointUI : MonoBehaviour
{
    public static PointUI Instance;
    [SerializeField] private TextMeshProUGUI pointText;
    [SerializeField] private MMF_Player pointFeedback;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void UpdateUI()
    {
        pointText.text =  DeliveryManager.Instance.GetSuccessfulRecipePoint().ToString();
        pointFeedback?.PlayFeedbacks();
    }
}
