using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    [SerializeField] private CompleteDishKitchenObject plateKitchenObject;
    [SerializeField] private IconTemplate iconTemplate;

    void Awake()
    {
        plateKitchenObject.OnIngredientAdded += plateKitchenObject_OnIngredientAdded;
    }

    private void Start()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void plateKitchenObject_OnIngredientAdded(object sender, CompleteDishKitchenObject.OnIngredientAddedEventArgs e)
    {
        UpdateVisual(e.addedIngredientKitchenObjectSO);
    }

    private void UpdateVisual(KitchenObjectSO addedIngredientSO)
    {
        if (addedIngredientSO.name == "Plate") return;

        IconTemplate plateIconTemplate = Instantiate<IconTemplate>(iconTemplate, this.transform);
        plateIconTemplate.gameObject.SetActive(true);
        plateIconTemplate.SetKitchenObjectSO(addedIngredientSO);
    }
}
