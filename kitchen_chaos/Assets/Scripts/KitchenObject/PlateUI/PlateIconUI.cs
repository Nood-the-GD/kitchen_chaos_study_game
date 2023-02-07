using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject plateKitchenObject;
    [SerializeField] private PlateIconTemplate iconTemplate;

    private void Start()
    {
        plateKitchenObject.OnIngredientAdded += plateKitchenObject_OnIngredientAdded;
    }

    private void plateKitchenObject_OnIngredientAdded(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        foreach(Transform child in transform)
        {
            if(child == iconTemplate) continue;
            else Destroy(child.gameObject);
        }

        foreach (KitchenObjectSO kitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
        {
            PlateIconTemplate plateIconTemplate = Instantiate<PlateIconTemplate>(iconTemplate, this.transform);
            plateIconTemplate.gameObject.SetActive(true);
            plateIconTemplate.SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}
