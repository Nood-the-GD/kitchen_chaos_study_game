using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
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
                if(player.GetKitchenObject() is PlateKitchenObject)
                {
                    //Player is holding a plate
                    if (player.GetKitchenObject().TryGetPlateKitchenObject(out PlateKitchenObject plateKitchenObject))
                    {
                        if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                        {
                            GetKitchenObject().DestroySelf();
                        }
                    }
                }
                else
                {
                    //Player is holding something not Plate
                    if(GetKitchenObject().TryGetPlateKitchenObject(out PlateKitchenObject plateKitchenObject))
                    {
                        //A plate is on this counter
                        if(plateKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO()))
                            player.GetKitchenObject().DestroySelf();
                    }
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
}
