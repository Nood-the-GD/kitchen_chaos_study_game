using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    public static event EventHandler OnSomethingPlacedHere;
    PhotonView photonView;
    public static void ResetStaticData()
    {
        OnSomethingPlacedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;
    private KitchenObject kitchenObject;

    public virtual void Interact(Player player){}
    public virtual void Chop(Player player){ }

    protected virtual void Awake(){
        photonView = GetComponent<PhotonView>();
    }

    #region Multiplay
    
    public void CmdInteract(int id){
        photonView.RPC("RPCIntertact", RpcTarget.All, id);
    }

    public void CmdChop(int id){
        photonView.RPC("RPCChop", RpcTarget.All, id);
    }

    [PunRPC]
    void RPCIntertact(int id){
        var player = PhotonManager.s.GetPlayerView(id);
        Interact(player);
    } 
    [PunRPC]
    void RPCChop(int id){
        var player = PhotonManager.s.GetPlayerView(id);
        Chop(player);
    }
    #endregion

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    } 

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if(kitchenObject != null)
        {
            OnSomethingPlacedHere?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return this.kitchenObject;
    }

    public void ClearKitchenObject()
    {
        this.kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
