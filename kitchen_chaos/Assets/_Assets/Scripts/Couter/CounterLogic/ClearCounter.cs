using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    public override void Interact(IKitchenObjectParent KOParent)
    {
        if (KOParent == null)
        {
            Debug.LogError("player is null");
            return;
        }

        if (HasKitchenObject())
        {

            GetKitchenObject().SetKitchenObjectParent(KOParent);
        }
        else
        {
            //Counter don't have kitchen object
            if (KOParent.HasKitchenObject())
            {
                //Player carrying something
                //Move kitchen object to counter
                KOParent.GetKitchenObject().SetKitchenObjectParent(this);
            }
            //else
            //Player carrying nothing
            //Do no thing
        }
    }

}
