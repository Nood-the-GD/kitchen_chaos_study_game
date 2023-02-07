using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlateIconTemplate : MonoBehaviour
{
    [SerializeField] private Image icon;

    private void Awake()
    {
        icon.gameObject.SetActive(false);
    }

    public void SetKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        icon.sprite = kitchenObjectSO.sprite;
    }
}
