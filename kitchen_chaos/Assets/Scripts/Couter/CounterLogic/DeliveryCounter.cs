using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public override void Interact(Player player)
    {
        if(player.HasKitchenObject())
        {
            if(player.GetKitchenObject().TryGetPlateKitchenObject(out PlateKitchenObject plateKitchenObject))
            {
                if(DeliveryManager.Instance.DeliverRecipe(plateKitchenObject))
                {
                    player.GetKitchenObject().DestroySelf();
                }
            }
        }
    }
}
