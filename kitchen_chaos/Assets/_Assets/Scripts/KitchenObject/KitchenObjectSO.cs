using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


public enum KitchenObjectType
{
    None,
    NeedChop,
    NeedFried,
}
[System.Serializable]
public class Recipe
{
    [Space(20)]
    [LabelText("Recipe Name", SdfIconType.Activity)]
    public string name;
    public List<KitchenObjectSO> ingredients;
    public KitchenObjectType actionType;
    public KitchenObjectSO output;
    public float step;

    public override string ToString()
    {
        return name;
    }
}


[CreateAssetMenu(fileName = "KitchenObject")]
public class KitchenObjectSO : ScriptableObject
{

    public Transform prefab;
    public Sprite sprite;
    public int point;
    public string objectName;
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
