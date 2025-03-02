using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;

    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(IKitchenContainable KOParent)
    {
        if (!KOParent.HasKitchenObject())
        {
            //Player carrying nothing
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, KOParent);
            //Debug.Log("Player grabbed object");
            OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
            OnInteract?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObjectSO GetContainerKitchenObject()
    {
        return kitchenObjectSO;
    }
}
