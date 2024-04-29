using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompleteDishManager : MonoBehaviour
{
    #region Instance
    public static CompleteDishManager Instance{get; private set;}
    #endregion

    #region Variables
    [SerializeField] private List<CompleteDishKitchenObject> completeDishList;
    #endregion

    void Awake()
    {
        Instance = this;
    }

    public bool TryCombineDish(KitchenObjectSO ingredient1, KitchenObjectSO ingredient2, out KitchenObjectSO resultDish) 
    {
        resultDish = null;
        if (completeDishList.Count == 0) return false;
        foreach(CompleteDishKitchenObject completeDishKitchenObject in completeDishList)
        {
            if(completeDishKitchenObject.IsHasIngredient(ingredient1) && completeDishKitchenObject.IsHasIngredient(ingredient2))
            {
                resultDish = completeDishKitchenObject.GetKitchenObjectSO();
                return true;
            }
        }
        return false;
    }
}
