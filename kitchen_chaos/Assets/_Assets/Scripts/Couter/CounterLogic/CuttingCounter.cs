using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgressBar, IAltInteractable
{
    #region Static
    public static event EventHandler OnCut;
    new public static void ResetStaticData()
    {
        OnCut = null;
    }
    #endregion

    #region Events
    public event EventHandler OnCutAction;
    public event EventHandler<IHasProgressBar.OnProcessChangedEvenArgs> OnProcessChanged;
    #endregion

    #region Variables 
    private int _cuttingProcess;
    public bool _isComplete;
    #endregion

    #region Interact
    public override void Interact(IKitchenContainable otherContainer)
    {

        base.Interact(otherContainer);

        if (!HasKitchenObject() && otherContainer.HasKitchenObject())
        {
            //Player carrying something
            //Move kitchen object to counter
            otherContainer.GetKitchenObject().SetContainerParent(this);
            if (GetKitchenObjectSO().CanCut())
            {
                _cuttingProcess = 0;
                _isComplete = false;
            }
        }
    }

    public void CmdChop(int id)
    {
        photonView.RPC("RPCChop", RpcTarget.All, id);
    }

    [PunRPC]
    public void RPCChop(int id)
    {
        var player = PhotonManager.s.GetPlayerView(id);
        Chop(player);
    }

    public void Chop(IKitchenContainable KOParent)
    {
        if(!GetKitchenObjectSO().CanCut()){
            Debug.Log("Can't cut");
        }
        if (HasKitchenObject() && GetKitchenObjectSO().CanCut())
        {
            //There is a kitchenObject on this counter and it can be cut.
            //Get output kitchenObject base on input with recipe.
            _cuttingProcess++;
            int cuttingProgressNumber = (int)GetKitchenObjectSO().GetCutOnlyRecipe().step;
            OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
            {
                processNormalize = (float)_cuttingProcess / cuttingProgressNumber
            });
            OnCutAction?.Invoke(this, EventArgs.Empty);
            OnCut?.Invoke(this, EventArgs.Empty);
            if (_cuttingProcess >= cuttingProgressNumber)
            {
                if (SectionData.s.isSinglePlay || KOParent.photonView.IsMine)
                {
                    KitchenObjectSO outputKitchenObject = GetKitchenObjectSO().GetCutOnlyRecipe().output;

                    //Destroy input kitchenObject.
                    GetKitchenObject().DestroySelf();

                    //Spawn output kitchenObject
                    KitchenObject.SpawnKitchenObject(outputKitchenObject, this);

                    _isComplete = true;
                }
            }
        }
        //else
        //There is a kitchenObject on this counter but can not be cut
        //Do nothing
    }

    public void AltInteract(IKitchenContainable kitchenObjectParent)
    {
        Debug.Log("AltInteract");
        Chop(kitchenObjectParent);
    }
    public bool CanAltInteract()
    {
        return !_isComplete;
    }
    #endregion

}
