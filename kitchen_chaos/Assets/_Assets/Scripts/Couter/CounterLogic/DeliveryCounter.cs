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

    public override void Interact(IKitchenContainable KOParent)
    {
        if(KOParent.HasKitchenObject())
        {
            if(KOParent.GetKitchenObject().TryGetCompleteDishKitchenObject(out CompleteDishKitchenObject completeDishKitchenObject))
            {
                if(!completeDishKitchenObject.IsHasPlate())
                {
                    Debug.Log("Doesn't have plate");
                    return;
                }

                if(DeliveryManager.Instance.DeliverRecipe(completeDishKitchenObject))
                {
                    KOParent.GetKitchenObject().DestroySelf();
                    deliverFeedback.PlayFeedbacks();
                }
                else
                {
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
