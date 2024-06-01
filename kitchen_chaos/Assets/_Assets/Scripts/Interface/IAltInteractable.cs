using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAltInteractable
{
    public void AltInteract(IKitchenObjectParent kitchenObjectParent);
    public bool CanAltInteract();
}
