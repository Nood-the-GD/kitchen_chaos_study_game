using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteDishKitchenObject : KitchenObject
{
    #region Events
    public event EventHandler<OnIngredientAddedEventArgs> OnIngredientAdded;
    public class OnIngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO addedIngredientKitchenObjectSO;
    }
    #endregion

    #region Variables
    [SerializeField] private List<KitchenObjectSO> ingredientsList;
    private List<KitchenObjectSO> kitchenObjectSOList = new List<KitchenObjectSO>();
    private bool isHasPlate = false;
    #endregion

    #region Support
    public bool IsHasIngredient(KitchenObjectSO kitchenObjectSO)
    {
        return ingredientsList.Contains(kitchenObjectSO);
    }
    public bool IsHasPlate()
    {
        return isHasPlate;
    }
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if(kitchenObjectSO.objectName == "Plate" && isHasPlate)
        {
            return false;
        }
        if (!ingredientsList.Contains(kitchenObjectSO) || kitchenObjectSOList.Contains(kitchenObjectSO))
        {
            return false;
        }

        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            addedIngredientKitchenObjectSO = kitchenObjectSO
        });
        if(kitchenObjectSO.objectName == "Plate")
        {
            // Do not add plate to kitchenObjectSOList
            isHasPlate = true;
            return true;
        }

        kitchenObjectSOList.Add(kitchenObjectSO);
        return true;
    }
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return kitchenObjectSOList;
    }
    #endregion
}
