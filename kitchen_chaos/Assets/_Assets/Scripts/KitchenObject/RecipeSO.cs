using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
    public List<KitchenObjectSO> kitchenObjectSOList;
    public string recipeName;
    public int Point 
    {
        get 
        {
            int point = 0;
            foreach(KitchenObjectSO kitchenObjectSO in kitchenObjectSOList)
            {
                point += kitchenObjectSO.point;
            }
            return point;
        }
    }
}
