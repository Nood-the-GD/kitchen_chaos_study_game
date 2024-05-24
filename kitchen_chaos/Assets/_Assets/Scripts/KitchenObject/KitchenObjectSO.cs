using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum KitchenObjectType
{
    NeedChop,
    NeedFried,
    Original 
}
[CreateAssetMenu(fileName = "KitchenObject")]
public class KitchenObjectSO : ScriptableObject
{

    public Transform prefab;
    public Sprite sprite;
    public int point;
    public string objectName;
    public KitchenObjectType kitchenObjectType;
}
