using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class BaseCounter : MonoBehaviour, IKitchenContainable
{
    public static EventHandler OnInteract;
    public static EventHandler OnAlternativeInteract;

    private long _lastInteractionTimestamp;
    private IKitchenContainable _previousContainer;
    
    PhotonView _photonView;
    public PhotonView photonView => _photonView;

    public KitchenObject kitchenObject { get; set; }

    public static event EventHandler OnSomethingPlacedHere;
    [SerializeField] private Transform counterTopPoint;
    public static void ResetStaticData()
    {
        OnSomethingPlacedHere = null;
        OnInteract = null;
        OnAlternativeInteract = null;
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
                // Store previous container before transferring
                _previousContainer = this;
                GetKitchenObject().CmdSetContainerParent(otherContainer);
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
                // Debug.Log("Combine result: " + combineResult.recipe.name);

                if (combineResult != null)
                {
                    otherContainer.ClearKitchenObject();

                    var recipe = combineResult.recipe;
                    var isHavingPlate = (kitchenObject != null && kitchenObject.IsHavingPlate) || (otherKO != null && otherKO.IsHavingPlate);
                    if (kitchenObject.GetKitchenObjectSO() != recipe.output)
                    {
                        ClearKitchenObject();


                        Debug.Log("Output: " + recipe.output.name);
                        //log all the ingredients
                        //current ingredients
                        foreach (var ingredient in combineResult.currentIngredients)
                        {
                            Debug.Log("Current Ingredient: " + ingredient.name);
                        }

                        foreach (var ingredient in combineResult.GetListOfIngredientsIndex())
                        {

                            Debug.Log("Ingredient: " + ingredient);
                        }
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
                otherContainer.GetKitchenObject().CmdSetContainerParent(this);
            }
        }
        OnInteract?.Invoke(this, EventArgs.Empty);
    }

    #region Multiplay

    public void CmdInteract(int id)
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _photonView.RPC(nameof(RPCInteract), RpcTarget.All, id, timestamp);
    }

    [PunRPC]
    public void RPCInteract(int id, long timestamp)
    {
        var player = PhotonManager.s.GetPlayerView(id);
        
        // Check for timestamp conflict - if another interaction happened very recently
        if (HasKitchenObject() && _lastInteractionTimestamp > 0 && 
            timestamp - _lastInteractionTimestamp < 100 && _previousContainer != null)
        {
            // We have a conflict - return the most recent object to its previous container
            if (timestamp > _lastInteractionTimestamp)
            {
                // Current interaction is newer, return this kitchen object to previous container
                GetKitchenObject().CmdSetContainerParent(_previousContainer);
                _previousContainer = null;
                _lastInteractionTimestamp = 0;
                return;
            }
        }
        
        _lastInteractionTimestamp = timestamp;
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
        // If we already have a kitchen object and are getting a new one
        if (this.kitchenObject != null && kitchenObject != null)
        {
            // Check for timestamp conflict
            long existingTimestamp = this.kitchenObject.interactionTimestamp;
            long newTimestamp = kitchenObject.interactionTimestamp;

            // If timestamps are close (within 100ms) we have a conflict
            if (Math.Abs(newTimestamp - existingTimestamp) < 100)
            {
                // Return the newer object to its previous container
                if (newTimestamp > existingTimestamp)
                {
                    // New object is more recent, return it to its previous parent
                    if (_previousContainer != null)
                    {
                        kitchenObject.CmdSetContainerParent(_previousContainer);
                        _previousContainer = null;
                        return;
                    }
                }
                else
                {
                    // Existing object is more recent, let the new one replace it
                    // (The existing one will be returned to previous in the normal flow)
                }
            }
        }

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

    public void ResetConflictData()
    {
        _lastInteractionTimestamp = 0;
        _previousContainer = null;
    }
    #endregion
}
