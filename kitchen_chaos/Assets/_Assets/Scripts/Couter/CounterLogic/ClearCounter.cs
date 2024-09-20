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

            if (!KOParent.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(KOParent);
            }
            else
            {
                //Player is carrying something
                if (KOParent.GetKitchenObject().GetKitchenObjectSO() is CompleteDishSO)
                {
                    //Player is holding a set of kitchen object
                    CompleteDishKitchenObject playerCompleteDish = KOParent.GetKitchenObject() as CompleteDishKitchenObject;
                    // Try add ingredient with the current kitchen object on counter
                    if (playerCompleteDish.TryAddIngredient(this.GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // After adding ingredient, destroy it on counter
                        GetKitchenObject().DestroySelf();
                    }
                }/*  */
                else
                {
                    TryAddPlayerIngredient(KOParent);
                }
            }
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

    private void TryAddPlayerIngredient(IKitchenObjectParent kitchenObjectParent)
    {
        KitchenObjectSO playerKitchenObjectSO = kitchenObjectParent.GetKitchenObject().GetKitchenObjectSO();
        if (GetKitchenObject() is CompleteDishKitchenObject)
        {
            // Kitchen object on counter is a set of kitchen object
            CompleteDishKitchenObject counterCompleteDish = GetKitchenObject() as CompleteDishKitchenObject;
            if (counterCompleteDish.TryAddIngredient(playerKitchenObjectSO))
            {
                kitchenObjectParent.GetKitchenObject().DestroySelf();
            }
        }
        else
        {
            // Kitchen object on counter is not a set of kitchen object
            // Try combine kitchen object on table and player
            if (CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
            {
                kitchenObjectParent.GetKitchenObject().DestroySelf();

                KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                KitchenObject.SpawnCompleteDish(resultDishSO, new KitchenObjectSO[] { playerKitchenObjectSO, counterKitchenObjectSO }, kitchenObjectParent);

                GetKitchenObject().DestroySelf();
            }
        }
    }
}
