using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateIconUI : MonoBehaviour
{
    private KitchenObject plateKitchenObject;
    [SerializeField] private IconTemplate iconTemplate;

    void Awake()
    {
        plateKitchenObject = transform.GetComponentInParent<KitchenObject>();
        plateKitchenObject.onAddIngredient += plateKitchenObject_OnIngredientAdded;
    }

    private void Start()
    {
        iconTemplate.gameObject.SetActive(false);
    }

    private void plateKitchenObject_OnIngredientAdded(List<KitchenObjectSO> ingredient)
    {
        UpdateVisual(ingredient);
    }

    private void UpdateVisual(List<KitchenObjectSO> addedIngredientSOs)
    {   
        var itemCount = transform.childCount;
        for(var i =1; i< itemCount+1; i ++){
            DestroyImmediate(transform.GetChild(1).gameObject);
        }

        foreach(var i in addedIngredientSOs){
            IconTemplate plateIconTemplate = Instantiate<IconTemplate>(iconTemplate, this.transform);
            plateIconTemplate.gameObject.SetActive(true);
            plateIconTemplate.SetKitchenObjectSO(i);
        }
        
    }
}
