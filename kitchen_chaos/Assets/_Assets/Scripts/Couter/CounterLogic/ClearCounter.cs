using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    private Player player;

    public override void Interact(Player player)
    {
        if(player == null){
            Debug.LogError("player is null");
            return;
        }

        if(HasKitchenObject())
        {
            //Counter has kitchen object
            if(!player.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else
            {
                //Player is carrying something
                if(player.GetKitchenObject() is CompleteDishKitchenObject)
                {
                    //Player is holding a set of kitchen object
                    CompleteDishKitchenObject playerCompleteDish = player.GetKitchenObject() as CompleteDishKitchenObject;
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
                    this.player = player;
                    TryAddPlayerIngredient();
                }
            }
        }
        else
        {
            //Counter don't have kitchen object
            if(player.HasKitchenObject())
            {
                //Player carrying something
                //Move kitchen object to counter
                player.GetKitchenObject().SetKitchenObjectParent(this);
            }
            //else
            //Player carrying nothing
            //Do no thing
        }
    }

    private void TryAddPlayerIngredient()
    {
        KitchenObjectSO playerKitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
        if(GetKitchenObject() is CompleteDishKitchenObject)
        {
            // Kitchen object on counter is a set of kitchen object
            CompleteDishKitchenObject counterCompleteDish = GetKitchenObject() as CompleteDishKitchenObject;
            if(counterCompleteDish.TryAddIngredient(playerKitchenObjectSO))
            {
                player.GetKitchenObject().DestroySelf();
            }
        }
        else
        {
            // Kitchen object on counter is not a set of kitchen object
            // Try combine kitchen object on table and player
            if(CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
            {
                player.GetKitchenObject().DestroySelf();
                KitchenObject.SpawnKitchenObject(resultDishSO, player);
                KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                GetKitchenObject().DestroySelf();

                CompleteDishKitchenObject completeDishKitchenObject = player.GetKitchenObject() as CompleteDishKitchenObject;

                Debug.Log(playerKitchenObjectSO.objectName + " " + counterKitchenObjectSO.objectName);
                completeDishKitchenObject.TryAddIngredient(playerKitchenObjectSO);
                completeDishKitchenObject.TryAddIngredient(counterKitchenObjectSO);

            }
        }
        // void KitchenObject_OnAnyKitchenObjectSpawned(KitchenObject completeDish)
        // {
        //         if(completeDish is not CompleteDishKitchenObject) return;

        //         KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();

        //         Debug.Log(playerKitchenObjectSO.objectName + " " + counterKitchenObjectSO.objectName);
        //         CompleteDishKitchenObject completeDishKitchenObject = completeDish as CompleteDishKitchenObject;
        //         completeDishKitchenObject.TryAddIngredient(playerKitchenObjectSO);
        //         completeDishKitchenObject.TryAddIngredient(counterKitchenObjectSO);

        //         GetKitchenObject().DestroySelf();
        //         // completeDish.SetKitchenObjectParent(player);
        //         this.player = null;
        // }
    }

}
