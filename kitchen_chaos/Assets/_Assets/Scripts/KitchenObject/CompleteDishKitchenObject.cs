using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    [FormerlySerializedAs("ingredientsList")]
    [SerializeField] private List<KitchenObjectSO> _ingredientsList;
    private List<KitchenObjectSO> _currentIngredients = new List<KitchenObjectSO>();
    private bool _isHasPlate = false;
    #endregion

    #region Support
    public bool IsHasIngredient(KitchenObjectSO kitchenObjectSO)
    {
        return _ingredientsList.Contains(kitchenObjectSO);
    }
    public bool IsHasPlate()
    {
        return _isHasPlate;
    }
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (kitchenObjectSO.objectName == "Plate" && _isHasPlate)
        {
            return false;
        }
        if (!_ingredientsList.Contains(kitchenObjectSO) || _currentIngredients.Contains(kitchenObjectSO))
        {
            return false;
        }

        OnIngredientAdded?.Invoke(this, new OnIngredientAddedEventArgs
        {
            addedIngredientKitchenObjectSO = kitchenObjectSO
        });
        if (kitchenObjectSO.objectName == "Plate")
        {
            // Do not add plate to kitchenObjectSOList
            _isHasPlate = true;
            return true;
        }

        _currentIngredients.Add(kitchenObjectSO);
        return true;
    }
    public List<KitchenObjectSO> GetKitchenObjectSOList()
    {
        return _currentIngredients;
    }
    public bool IsCorrectRecipe(RecipeSO recipeSO)
    {
        if (recipeSO.kitchenObjectSOList.Count != _currentIngredients.Count) return false;
        foreach (KitchenObjectSO kitchenObjectSO in _currentIngredients)
        {
            if (!recipeSO.kitchenObjectSOList.Contains(kitchenObjectSO))
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}
