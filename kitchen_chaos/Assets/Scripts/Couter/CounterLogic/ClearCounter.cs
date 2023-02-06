using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearCounter : BaseCounter
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    public override void Interact(Player player)
    {
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
                    PlateKitchenObject plateKitchenObject;
                    if (player.GetKitchenObject().TryGetPlateKitchenObject(out plateKitchenObject))
                    {
                        if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                        {
                            GetKitchenObject().DestroySelf();
                        }
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
