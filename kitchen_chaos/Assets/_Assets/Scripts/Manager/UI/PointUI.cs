using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PointUI : MonoBehaviour
{
    public static PointUI Instance;
    [SerializeField] private TextMeshProUGUI pointText;

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
        pointText.text =  DeliveryManager.Instance.GetSuccessfulRecipeAmount().ToString();
    }
}
