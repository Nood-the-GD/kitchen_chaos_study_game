using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class BaseCounter : MonoBehaviour, IContainable
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

    public virtual void Interact(IContainable otherContainer) { 
        if(otherContainer == null){
            Debug.LogError("player is null");
            return;
        }

        if(HasKitchenObject())
        {
            //Player is not holding
            if(!otherContainer.HasKitchenObject())
            {
                Debug.Log("player is not holding");
                GetKitchenObject().SetContainerParent(otherContainer);
            }
            else
            {
                Debug.Log("player is holding");
                //Player is carrying something
                if(otherContainer.GetKitchenObject() is CompleteDishKitchenObject)
                {
                    //Player is holding a set of kitchen object
                    CompleteDishKitchenObject playerCompleteDish = otherContainer.GetKitchenObject() as CompleteDishKitchenObject;
                    // Try add ingredient with the current kitchen object on counter
                    if(playerCompleteDish.TryAddIngredient(this.GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // After adding ingredient, destroy it on counter
                        GetKitchenObject().DestroySelf();
                    }
                }
                else
                {
                    //Player is holding an ingredient
                    TryAddPlayerIngredient(otherContainer.GetKitchenObject());
                }
            }
        }
        else
        {
            //Counter don't have kitchen object
            if(otherContainer.HasKitchenObject())
            {
                //Player carrying something
                //Move kitchen object to counter
                otherContainer.GetKitchenObject().SetContainerParent(this);
            }
        }

    }

    private void TryAddPlayerIngredient(KitchenObject playerKitchenObject)
    {
        var playerObjectSO = playerKitchenObject.GetKitchenObjectSO();
        if(GetKitchenObject() is CompleteDishKitchenObject)
        {
            // Kitchen object on counter is a set of kitchen object
            CompleteDishKitchenObject counterCompleteDish = GetKitchenObject() as CompleteDishKitchenObject;
            if(counterCompleteDish.TryAddIngredient(playerObjectSO))
            {
               playerKitchenObject.DestroySelf();
            }
        }
        else
        {
            if(CompleteDishManager.Instance.TryCombineDish(playerObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
            {
                playerKitchenObject.DestroySelf();
                KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                KitchenObject.SpawnCompleteDish(resultDishSO, new KitchenObjectSO[] {playerObjectSO, counterKitchenObjectSO}, this);

                GetKitchenObject().DestroySelf();
            }
        }
    }

    protected virtual void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    #region Multiplay

    public void CmdInteract(int id)
    {
        _photonView.RPC("RPCIntertact", RpcTarget.All, id);
    }



    [PunRPC]
    public void RPCIntertact(int id)
    {
        var player = PhotonManager.s.GetPlayerView(id);
        Interact(player);
    }

    #endregion

    public Transform GetKitchenObjectFollowTransform()
    {
        return counterTopPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        if (kitchenObject != null)
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

    public KitchenObjectSO GetKitchenObjectSO(){
        return this.kitchenObject.GetKitchenObjectSO();
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
