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


#if UNITY_EDITOR
    bool isEnoughName
    {
        get
        {
            return CheckEnoughName();
        }
    }

    [Button]
    public void SortRecipe()
    {
        recipes.Sort((a, b) => a.ingredients.Count.CompareTo(b.ingredients.Count));
    }

    [Button]
    public void AddNewRecipe()
    {
        var newRecipe = new Recipe();
        newRecipe.ingredients = new List<KitchenObjectSO>();
        newRecipe.output = new KitchenObjectSO();
        newRecipe.actionType = KitchenObjectType.None;
        recipes.Add(newRecipe);
    }

    [Button(ButtonSizes.Large)]
    public void SetNameForAllRecipe()
    {
        foreach (var recipe in recipes)
        {
            recipe.name = recipe.output.name;
        }
    }

    [Button(ButtonSizes.Large), GUIColor(0, 1, 0), ShowIf("@isEnoughName")]
    public void MakeEnumFromRecipe()
    {
        var folderPath = "Assets/_Assets/Scripts/Enums";
        var fileName = "RecipeEnum";
        var filePath = folderPath + "/" + fileName + ".cs";
        var fileContent = "public enum " + fileName + "\n{\n";
        foreach (var recipe in recipes)
        {
            fileContent += recipe.name + ",\n";
        }
        fileContent += "}\n";
        System.IO.File.WriteAllText(filePath, fileContent);
    }
    private bool CheckEnoughName()
    {
        foreach (var recipe in recipes)
        {
            if (string.IsNullOrEmpty(recipe.name))
            {
                return false;
            }
        }
        return true;
    }
#endif


    public Recipe GetFryingRecipe(KitchenObjectSO input)
    {
        return recipes.Find(x => x.ingredients.Contains(input) && x.actionType == KitchenObjectType.NeedFried);
    }

    public Recipe GetCuttingRecipe(KitchenObjectSO input)
    {
        return recipes.Find(x => x.ingredients.Contains(input) && x.actionType == KitchenObjectType.NeedChop);
    }
}
