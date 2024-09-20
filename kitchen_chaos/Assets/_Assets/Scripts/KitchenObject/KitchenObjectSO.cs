using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum KitchenObjectType
{
    None,
    NeedChop,
    NeedFried,
    Original
}
[System.Serializable]
public class Recipe
{
    public List<KitchenObjectSO> ingredients;
    public KitchenObjectType actionType;
    public KitchenObjectSO output;
    public KitchenObjectSO overCookOutput;
    public float step;
}


[CreateAssetMenu(fileName = "KitchenObject")]
public class KitchenObjectSO : ScriptableObject
{

    public Transform prefab;
    public Sprite sprite;
    public int point;
    public string objectName;
    public List<Recipe> recipes = new List<Recipe>();


    public Recipe GetCutOnlyRecipe()
    {
        return recipes.Find(x => x.actionType == KitchenObjectType.NeedChop && x.ingredients.Count == 0);
    }

    public Recipe GetFriedOnlyRecipe()
    {
        return recipes.Find(x => x.actionType == KitchenObjectType.NeedFried && x.ingredients.Count == 1);
    }

    public bool IsCanCut()
    {
        return GetCutOnlyRecipe() != null;
    }

    public bool IsCanFried()
    {
        return GetFriedOnlyRecipe() != null;
    }



}
