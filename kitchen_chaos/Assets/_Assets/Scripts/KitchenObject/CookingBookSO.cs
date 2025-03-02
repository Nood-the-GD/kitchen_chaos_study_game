using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public class CombineResult
{
    public List<KitchenObjectSO> currentIngredients;
    public RecipeSO recipe;

    public List<int> GetListOfIngredientsIndex()
    {
        var indexes = new List<int>();
        foreach (var i in currentIngredients)
        {
            indexes.Add(recipe.ingredients.IndexOf(i));
        }
        return indexes;
    }
}


[CreateAssetMenu(fileName = "CookingBookSO", menuName = "ScriptableObjects/CookingBookSO", order = 1)]
public class CookingBookSO : ScriptableObject
{
    [Searchable]
    public List<RecipeSO> recipes = new List<RecipeSO>();

    public bool isSorted { get; private set; } = false;
    private static CookingBookSO _s;
    public static CookingBookSO s
    {
        get
        {
            if (_s == null)
            {
                _s = Resources.Load<CookingBookSO>("CookingBookSO");
                if (_s.isSorted == false)
                {
                    _s.Sort();
                }
                if (_s == null)
                    Debug.LogError("CookingBookSO not found");
            }
            return _s;
        }
    }


    public RecipeSO FindRecipeByOutput(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var i in recipes)
        {
            if (i.output == kitchenObjectSO)
                return i;
        }
        return null;
    }

    public List<KitchenObjectSO> GetRecursiveIngredients(KitchenObjectSO kitchenObjectSO)
    {
        // Attempt to find a recipe that outputs this kitchen object.
        RecipeSO recipe = FindRecipeByOutput(kitchenObjectSO);

        // If no recipe exists, it's a base ingredient.
        if (recipe == null)
        {
            return new List<KitchenObjectSO>() { kitchenObjectSO };
        }


        //base ingredients
        if (recipe.ingredients.Count == 1)
        {
            return new List<KitchenObjectSO>() { recipe.output };
        }

        // Otherwise, create a list to hold all the base ingredients.
        List<KitchenObjectSO> baseIngredients = new List<KitchenObjectSO>();

        // Iterate over each ingredient in the recipe.
        foreach (var ingredient in recipe.ingredients)
        {
            // Recursively get the ingredients for this ingredient.
            List<KitchenObjectSO> ingredientList = GetRecursiveIngredients(ingredient);
            baseIngredients.AddRange(ingredientList);
        }

        return baseIngredients;
    }


    public CombineResult TryCombine(KitchenObject kitchenObject, KitchenObject kitchenObject1)
    {
        return TryCombine(kitchenObject.GetKitchenObjectSO(), kitchenObject1.GetKitchenObjectSO());
    }
    public CombineResult TryCombine(KitchenObjectSO kitchenObjectSO, KitchenObjectSO kitchenObjectSO1)
    {
        var i1 = GetRecursiveIngredients(kitchenObjectSO);
        var i2 = GetRecursiveIngredients(kitchenObjectSO1);
        i1.AddRange(i2);
        var find = recipes.Find(x => x.IsSubsetIngredients(i1));
        if (find != null)
        {
            return new CombineResult()
            {
                currentIngredients = i1,
                recipe = find
            };
        }

        Debug.Log("Combine failed. Ingredients:");
        foreach (var i in i1)
        {
            Debug.Log(i.name);
        }

        return null;
    }


#if UNITY_EDITOR
    [Button(ButtonSizes.Large)]
    private void FindAllRecipe()
    {
        this.recipes.Clear();
        var folderPath = "RecipeSo";
        var recipes = Resources.LoadAll<RecipeSO>(folderPath);
        this.recipes = new List<RecipeSO>(recipes);
    }
    [Button(ButtonSizes.Large)]
    private void Sort()
    {
        recipes.Sort((a, b) => a.ingredients.Count.CompareTo(b.ingredients.Count));
        isSorted = true;
    }
#endif


    public RecipeSO GetFryingRecipe(KitchenObjectSO input)
    {
        return recipes.Find(x => x.ingredients.Contains(input) && x.ingredients.Count == 1 && x.actionType == KitchenObjectType.NeedFried);
    }

    public RecipeSO GetCuttingRecipe(KitchenObjectSO input)
    {
        return recipes.Find(x => x.ingredients.Contains(input) && x.ingredients.Count == 1 && x.actionType == KitchenObjectType.NeedChop);
    }
}
