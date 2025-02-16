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


[CreateAssetMenu(fileName = "KitchenObject")]
public class KitchenObjectSO : ScriptableObject
{

    public Transform prefab;
    public Sprite sprite;
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
