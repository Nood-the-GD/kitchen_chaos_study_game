using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;
    [SerializeField] private Transform counterTopPoint;

    public void Interact()
    {
        Transform kitchenObjectTransform = Instantiate(this.kitchenObjectSO.prefab, counterTopPoint);
        kitchenObjectTransform.localPosition = Vector3.zero;

        KitchenObjectSO kitchenObjectSO = kitchenObjectTransform.GetComponent<KitchenObject>().GetKitchenObjectSO();
        Debug.Log(kitchenObjectSO.name);
    }


}
