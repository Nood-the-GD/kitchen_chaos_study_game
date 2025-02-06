using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public interface IKitchenContainable
{
    public Transform GetKitchenObjectFollowTransform();

    public void SetKitchenObject(KitchenObject kitchenObject);

    public KitchenObject GetKitchenObject();

    public void ClearKitchenObject();

    public bool HasKitchenObject();

    public PhotonView photonView { get; }
}
