using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAltInteractable
{
    public void AltInteract(IContainable kitchenObjectParent);
    public bool CanAltInteract();
    public bool HasKitchenObject();
}
