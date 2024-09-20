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
        if (Instance == null) Instance = this;
    }

    public override void Interact(IKitchenObjectParent KOParent)
    {
        if (KOParent.HasKitchenObject())
        {
            if (KOParent.GetKitchenObject())
            {

                if (DeliveryManager.Instance.DeliverRecipe(KOParent.GetKitchenObject()))
                {
                    KOParent.GetKitchenObject().DestroySelf();
                    deliverFeedback.PlayFeedbacks();
                }
                else
                {
                    Debug.Log("Delivery failed");
                }
            }
            else
            {
                Debug.Log("Player is carrying something not Plate");

            }
        }
        else
        {
            Debug.Log("Player is not carrying anything");
        }
    }
}
