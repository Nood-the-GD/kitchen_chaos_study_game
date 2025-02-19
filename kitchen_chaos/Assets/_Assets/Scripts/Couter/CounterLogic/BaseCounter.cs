using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class BaseCounter : MonoBehaviour, IKitchenContainable
{
    PhotonView _photonView;
    public PhotonView photonView => _photonView;

    public KitchenObject kitchenObject { get; set; }

    public static event EventHandler OnSomethingPlacedHere;
    [SerializeField] private Transform counterTopPoint;
    public static void ResetStaticData()
    {
        OnSomethingPlacedHere = null;
    }

    protected virtual void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    public virtual void Interact(IKitchenContainable otherContainer)
    {
        Debug.Log("Interact");

        if (otherContainer == null)
        {
            Debug.LogError("player is null");
            return;
        }

        if (HasKitchenObject())
        {
            //Player is not holding
            if (!otherContainer.HasKitchenObject())
            {
                Debug.Log("player is not holding");
                GetKitchenObject().SetContainerParent(otherContainer);
            }
            else
            {
                var otherKO = otherContainer.GetKitchenObject();

                //holding plate
                if (otherKO.IsPlate)
                {
                    if (kitchenObject.TryAddPlate())
                    {
                        otherContainer.ClearKitchenObject();
                        return;
                    }
                }
                if (GetKitchenObject().IsPlate)
                {
                    if (otherContainer.GetKitchenObject().TryAddPlate())
                    {
                        ClearKitchenObject();
                        return;
                    }
                }

                var combineResult = CookingBookSO.s.TryCombine(otherKO, GetKitchenObject());
                Debug.Log("Combine result: " + combineResult.recipe.name);

                if (combineResult != null)
                {
                    otherContainer.ClearKitchenObject();

                    var recipe = combineResult.recipe;
                    var isHavingPlate = (kitchenObject != null && kitchenObject.IsHavingPlate) || (otherKO != null && otherKO.IsHavingPlate);
                    if (kitchenObject.GetKitchenObjectSO() != recipe.output)
                    {
                        ClearKitchenObject();


                        Debug.Log("Output: " + recipe.output.name);
                        KitchenObject.SpawnKitchenObject(recipe.output, this, combineResult.GetListOfIngredientsIndex(), isHavingPlate);
                    }
                    else
                    {
                        kitchenObject.AddIngredient(otherKO.GetKitchenObjectSO(), isHavingPlate);
                        ClearKitchenObject();
                    }

                }

            }
        }
        else //Counter don't have kitchen object
        {
            if (otherContainer.HasKitchenObject())
            {
                //Player carrying something
                otherContainer.GetKitchenObject().SetContainerParent(this);
            }
        }
    }

    #region Multiplay

    public void CmdInteract(int id)
    {
        _photonView.RPC(nameof(RPCInteract), RpcTarget.All, id);
    }

    [PunRPC]
    public void RPCInteract(int id)
    {
        var player = PhotonManager.s.GetPlayerView(id);
        Interact(player);
    }

    #endregion

    #region Public Methods
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

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return this.kitchenObject.GetKitchenObjectSO();
    }


    public void ClearKitchenObject(bool destroyKO = true)
    {

        if (kitchenObject != null && destroyKO)
            kitchenObject.DestroySelf();

        this.kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
    #endregion
}
