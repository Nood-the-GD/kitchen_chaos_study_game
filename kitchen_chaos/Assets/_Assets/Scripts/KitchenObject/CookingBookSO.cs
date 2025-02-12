using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "CookingBookSO", menuName = "ScriptableObjects/CookingBookSO", order = 1)]
public class CookingBookSO : ScriptableObject
{
    [Searchable]
    public List<Recipe> recipes = new List<Recipe>();

    private static CookingBookSO _s;
    public static CookingBookSO s
    {
        get
        {
            if (_s == null)
            {
                _s = Resources.Load<CookingBookSO>("CookingBookSO");
                if (_s == null)
                    Debug.LogError("CookingBookSO not found");
            }
            return _s;
        }
    }

    public Recipe GetFryingRecipe(KitchenObjectSO input)
    {
        return recipes.Find(x => x.ingredients.Contains(input) && x.actionType == KitchenObjectType.NeedFried);
    }

    public Recipe GetCuttingRecipe(KitchenObjectSO input)
    {
        return recipes.Find(x => x.ingredients.Contains(input) && x.actionType == KitchenObjectType.NeedChop);
    }
}
