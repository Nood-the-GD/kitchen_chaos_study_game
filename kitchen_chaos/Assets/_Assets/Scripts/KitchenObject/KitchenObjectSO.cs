using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

public enum KitchenObjectType
{
    None,
    NeedChop,
    NeedFried,
}
[System.Serializable]
public class Recipe
{
    [FoldoutGroup("@name")]
    [Space(20)]
    [LabelText("Recipe Name", SdfIconType.Activity)]
    public string name;
    [FoldoutGroup("@name")]
    public List<KitchenObjectSO> ingredients;
    [FoldoutGroup("@name")]
    public KitchenObjectType actionType;
    [FoldoutGroup("@name")]
    public KitchenObjectSO output;
    [FoldoutGroup("@name")]
    public float step;

    public bool isStackable = true;
    //bool isStackable => ingredients.Count > 2;


    public int point = 5;

    public override string ToString()
    {
        return name;
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


[CreateAssetMenu(fileName = "KitchenObject")]
public class KitchenObjectSO : ScriptableObject
{

    public Transform prefab;
    public Sprite sprite;
    public int point;
    public string objectName;
    [EnumToggleButtons]
    public KitchenObjectType kitchenObjectType;

    public bool CanCut()
    {
        return kitchenObjectType == KitchenObjectType.NeedChop;
    }

    public bool CanFried()
    {
        return kitchenObjectType == KitchenObjectType.NeedFried;
    }
}
