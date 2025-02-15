using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteDishVisual : MonoBehaviour
{
    [SerializeField] private CompleteDishKitchenObject completeDishKitchenObject;
    [SerializeField] private List<KitchenObjectSO_GameObject> kitchenObjectSO_GameObjectsList;

    void Awake()
    {
        completeDishKitchenObject.OnIngredientAdded += CompleteDish_OnIngredientAdded;
        foreach (KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectsList)
        {
            kitchenObjectSO_GameObject.gameObject.SetActive(false);
        }
    }

    private void CompleteDish_OnIngredientAdded(object sender, CompleteDishKitchenObject.OnIngredientAddedEventArgs ingredient)
    {
        // Debug.Log("On added ingredient ");
        foreach (KitchenObjectSO_GameObject kitchenObjectSO_GameObject in kitchenObjectSO_GameObjectsList)
        {
            if (kitchenObjectSO_GameObject.kitchenObjectSO == ingredient.addedIngredientKitchenObjectSO)
            {
                kitchenObjectSO_GameObject.gameObject.SetActive(true);
            }
        }
    }
}
