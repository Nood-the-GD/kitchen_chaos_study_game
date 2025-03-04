using MoreMountains.Feedbacks;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    [SerializeField] private DeliveryCounterVisual deliveryCounterVisual;
    public static DeliveryCounter Instance;
    [SerializeField] MMF_Player deliverFeedback;

    protected override void Awake()
    {
        base.Awake();
        if (Instance == null) Instance = this;
    }

    public override void Interact(IKitchenContainable KOParent)
    {
        if (KOParent.HasKitchenObject())
        {
            if (DeliveryManager.Instance.DeliverFood(KOParent.GetKitchenObject()))
            {
                KOParent.ClearKitchenObject();
                deliverFeedback.PlayFeedbacks();
            }
            else
            {
                string message = "";
                if (KOParent.GetKitchenObject().IsHaveEnoughIngredient() == false)
                {
                    message = "Not enough ingredient";
                }
                else if (DeliveryManager.Instance.OrderList.Contains(KOParent.GetKitchenObject().GetKitchenObjectSO()) == false)
                {
                    message = "Not in the order list";
                }
                else if (KOParent.GetKitchenObject().IsHavingPlate == false)
                {
                    message = "Need plate";
                }
                deliveryCounterVisual.ShowMessage(message);
            }
        }
        else
        {
            Debug.Log("Player is not carrying anything");
        }
    }
}
