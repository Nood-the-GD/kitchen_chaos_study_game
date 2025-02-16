using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "KitchenObject")]
public class RecipeSO:ScriptableObject
{

    public string nameRec;
    public List<KitchenObjectSO> ingredients;

    public KitchenObjectType actionType;

    public KitchenObjectSO output;

    public float step;



    public int point = 5;

    public override string ToString()
    {
        return nameRec;
    }

    public bool IsSameIngredients(List<KitchenObjectSO> kitchenObjectSOs)
    {
        // Ensure both lists are not null and then use SequenceEqual for a one-line comparison
        return kitchenObjectSOs != null && ingredients != null &&
            ingredients.SequenceEqual(kitchenObjectSOs);
    }

    public bool IsSubsetIngredients(List<KitchenObjectSO> kitchenObjectSOs)
    {
        // Check that the provided list is not null and all its items exist in the ingredients list
        return kitchenObjectSOs != null && ingredients != null &&
            kitchenObjectSOs.All(item => ingredients.Contains(item));
    }
}
