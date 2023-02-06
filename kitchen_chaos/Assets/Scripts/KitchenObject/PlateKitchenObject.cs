using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    private List<KitchenObjectSO> kitchenObjectSOList;

    [SerializeField] List<KitchenObjectSO> validKitchenOSList;

    private void Awake()
    {
        kitchenObjectSOList = new List<KitchenObjectSO>();
    }

    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (validKitchenOSList.Contains(kitchenObjectSO))
        {
            kitchenObjectSOList.Add(kitchenObjectSO);
            return true;
        }
        return false;
    }
}
