using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    public static DeliveryCounter Instance;
    [SerializeField] MMF_Player deliverFeedback;

    protected override void Awake() 
    {
        base.Awake();
        if(Instance == null) Instance = this;
    }

    public override void Interact(Player player)
    {
        if(player.HasKitchenObject())
        {
            if(player.GetKitchenObject().TryGetPlateKitchenObject(out PlateKitchenObject plateKitchenObject))
            {
                if(DeliveryManager.Instance.DeliverRecipe(plateKitchenObject))
                {
                    player.GetKitchenObject().DestroySelf();
                    deliverFeedback.PlayFeedbacks();
                }
                else{
                    Debug.Log("Delivery failed");
                }
            }
            else{
                Debug.Log("Player is carrying something not Plate");
            
            }
        }
        else{
            Debug.Log("Player is not carrying anything");
        }
    }
}
