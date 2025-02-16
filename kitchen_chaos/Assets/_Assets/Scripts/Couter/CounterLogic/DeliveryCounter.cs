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
            if(DeliveryManager.Instance.DeliverFood(KOParent.GetKitchenObject()))
            {
                KOParent.ClearKitchenObject();
                deliverFeedback.PlayFeedbacks();
            }
        }
        else{
            Debug.Log("Player is not carrying anything");
        }
    }
}
