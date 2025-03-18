using System.Security.Cryptography;
using MoreMountains.Feedbacks;
using Photon.Pun;
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
                CmdFeedback();
            }
            else
            {
                string message = "";
                if (KOParent.GetKitchenObject().IsHavingPlate == false)
                {
                    message = "Need plate";
                }
                else{
                    message = "Not in the order list";
                }
                CmdShowMessage(message);
            }
        }
        else
        {
            Debug.Log("Player is not carrying anything");
        }
    }

    private void CmdShowMessage(string message)
    {
        photonView.RPC(nameof(RpcShowMessage), RpcTarget.All, message);
    }

    [PunRPC]
    private void RpcShowMessage(string message)
    {
        deliveryCounterVisual.ShowMessage(message);
    }

    private void CmdFeedback()
    {
        photonView.RPC(nameof(RpcFeedback), RpcTarget.All);
    }

    [PunRPC]
    private void RpcFeedback()
    {
        deliverFeedback.PlayFeedbacks();
    }
}
