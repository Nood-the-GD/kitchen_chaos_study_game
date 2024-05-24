using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    private IKitchenObjectParent kitchenObjectParent;

    public override void Interact(IKitchenObjectParent KOParent)
    {
        if(KOParent == null){
            Debug.LogError("player is null");
            return;
        }

        if(HasKitchenObject())
        {
            //Counter has kitchen object
            if(!KOParent.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(KOParent);
            }
            else
            {
                //Player is carrying something
                if(KOParent.GetKitchenObject() is CompleteDishKitchenObject)
                {
                    //Player is holding a set of kitchen object
                    CompleteDishKitchenObject playerCompleteDish = KOParent.GetKitchenObject() as CompleteDishKitchenObject;
                    // Try add ingredient with the current kitchen object on counter
                    if(playerCompleteDish.TryAddIngredient(this.GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // After adding ingredient, destroy it on counter
                        GetKitchenObject().DestroySelf();
                    }
                }/*  */
                else
                {
                    //Player is holding an ingredient
                    this.kitchenObjectParent = KOParent;
                    TryAddPlayerIngredient();
                }
            }
        }
        else
        {
            //Counter don't have kitchen object
            if(KOParent.HasKitchenObject())
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

    private void TryAddPlayerIngredient()
    {
        KitchenObjectSO playerKitchenObjectSO = kitchenObjectParent.GetKitchenObject().GetKitchenObjectSO();
        if(GetKitchenObject() is CompleteDishKitchenObject)
        {
            // Kitchen object on counter is a set of kitchen object
            CompleteDishKitchenObject counterCompleteDish = GetKitchenObject() as CompleteDishKitchenObject;
            if(counterCompleteDish.TryAddIngredient(playerKitchenObjectSO))
            {
                kitchenObjectParent.GetKitchenObject().DestroySelf();
            }
        }
        else
        {
            // Kitchen object on counter is not a set of kitchen object
            // Try combine kitchen object on table and player
            if(CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
            {
                kitchenObjectParent.GetKitchenObject().DestroySelf();

                KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                KitchenObject.SpawnCompleteDish(resultDishSO, new KitchenObjectSO[] {playerKitchenObjectSO, counterKitchenObjectSO}, kitchenObjectParent);

                GetKitchenObject().DestroySelf();
            }
        }
    }
}
