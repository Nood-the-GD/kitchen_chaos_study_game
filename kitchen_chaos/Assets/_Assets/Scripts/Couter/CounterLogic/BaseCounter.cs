using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class BaseCounter : MonoBehaviour, IKitchenObjectParent
{
    PhotonView _photonView;
    public PhotonView photonView => _photonView;
    public static event EventHandler OnSomethingPlacedHere;
    public static void ResetStaticData()
    {
        OnSomethingPlacedHere = null;
    }

    [SerializeField] private Transform counterTopPoint;
    private KitchenObject kitchenObject;

    public virtual void Interact(IKitchenObjectParent KOParent){}
    public virtual void Chop(IKitchenObjectParent KOParent){ }

    protected virtual void Awake(){
        _photonView = GetComponent<PhotonView>();
    }

    #region Multiplay
    
    public void CmdInteract(int id){
        _photonView.RPC("RPCIntertact", RpcTarget.All, id);
    }

    public void CmdChop(int id){
        _photonView.RPC("RPCChop", RpcTarget.All, id);
    }

    [PunRPC]
    public void RPCIntertact(int id){
        var player = PhotonManager.s.GetPlayerView(id);
        Interact(player);
    } 
    [PunRPC]
    public void RPCChop(int id){
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
        if(kitchenObject != null)
        {
            Debug.Log(kitchenObject.gameObject.name);
            OnSomethingPlacedHere?.Invoke(this, EventArgs.Empty);
        }

        this.kitchenObject = kitchenObject;
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
