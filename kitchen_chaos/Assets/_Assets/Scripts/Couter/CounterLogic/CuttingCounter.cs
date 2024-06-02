using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;
    private int _cuttingProcess;
    private IKitchenObjectParent KOParent;
    public bool _isComplete;
    #endregion

    #region Interact
    public override void Interact(IKitchenObjectParent KOParent)
    {
        if(KOParent == null){
            Debug.LogError("player is null");
            return;
        }

        if(HasKitchenObject())
        {
            //Counter has kitchen object
            if(!KOParent.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(KOParent);
                _isComplete = true;
            }
            else
            {
                //Player is carrying something
                if(KOParent.GetKitchenObject() is CompleteDishKitchenObject)
                {
                    //Player is holding a set of kitchen object
                    CompleteDishKitchenObject playerCompleteDish = KOParent.GetKitchenObject() as CompleteDishKitchenObject;
                    // Try add ingredient with the current kitchen object on counter
                    if(playerCompleteDish.TryAddIngredient(this.GetKitchenObject().GetKitchenObjectSO()))
                    {
                        // After adding ingredient, destroy it on counter
                        GetKitchenObject().DestroySelf();
                        _isComplete = true;
                    }
                }
                else
                {
                    //Player is holding an ingredient
                    this.KOParent = KOParent;
                    TryAddPlayerIngredient();
                }
            }
        }
        else
        {
            //Counter don't have kitchen object
            if(KOParent.HasKitchenObject())
            {
                //Player carrying something
                //Move kitchen object to counter
                KOParent.GetKitchenObject().SetKitchenObjectParent(this);
                if(HasRecipeForInput(GetKitchenObject().GetKitchenObjectSO()))
                {
                    _cuttingProcess = 0;
                    _isComplete = false;
                }
            }
            //else
            //Player carrying nothing
            //Do no thing
        }
    }
    public override void Chop(IKitchenObjectParent KOParent)
    {
        if(HasKitchenObject() && HasRecipeForInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //There is a kitchenObject on this counter and it can be cut.
            //Get output kitchenObject base on input with recipe.
            _cuttingProcess++;
            int cuttingProgressNumber = GetOutputProcessForInput(GetKitchenObject().GetKitchenObjectSO());
            OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
            {
                processNormalize = (float)_cuttingProcess / cuttingProgressNumber
            });

            OnCutAction?.Invoke(this, EventArgs.Empty);
            OnCut?.Invoke(this, EventArgs.Empty);
            if (_cuttingProcess >= cuttingProgressNumber)
            {
                if(SectionData.s.isSinglePlay || KOParent.photonView.IsMine)
                {
                    KitchenObjectSO outputKitchenObject = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

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

    public void AltInteract(IKitchenObjectParent kitchenObjectParent)
    {
        Chop(kitchenObjectParent);
    }
    public bool CanAltInteract()
    {
        return !_isComplete;
    }
    #endregion

    private void TryAddPlayerIngredient()
    {
        KitchenObjectSO playerKitchenObjectSO = KOParent.GetKitchenObject().GetKitchenObjectSO();
        if(GetKitchenObject() is CompleteDishKitchenObject)
        {
            // Kitchen object on counter is a set of kitchen object
            CompleteDishKitchenObject counterCompleteDish = GetKitchenObject() as CompleteDishKitchenObject;
            if(counterCompleteDish.TryAddIngredient(playerKitchenObjectSO))
            {
                KOParent.GetKitchenObject().DestroySelf();
                _isComplete = true;
            }
        }
        else
        {
            // Kitchen object on counter is not a set of kitchen object
            // Try combine kitchen object on table and player
            if(CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
            {
                KOParent.GetKitchenObject().DestroySelf();

                KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                KitchenObject.SpawnCompleteDish(resultDishSO, new KitchenObjectSO[] {playerKitchenObjectSO, counterKitchenObjectSO}, KOParent);

                GetKitchenObject().DestroySelf();
                _isComplete = true;
            }
        }
    }

    public KitchenObjectSO GetCuttingInputForOutput(KitchenObjectSO output)
    {
        foreach (CuttingRecipeSO recipe in cuttingRecipeSOArray)
        {
            if (recipe.output == output) return recipe.input;
        }
        return null;
    }

    private CuttingRecipeSO GetCuttingRecipeForInput(KitchenObjectSO input)
    {
        foreach (CuttingRecipeSO recipe in cuttingRecipeSOArray)
        {
            if (recipe.input == input) return recipe;
        }
        return null;
    }

    private bool HasRecipeForInput(KitchenObjectSO input)
    {
        return GetCuttingRecipeForInput(input) != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO input)
    {
        CuttingRecipeSO outputCuttingRecipe = GetCuttingRecipeForInput(input);
        return outputCuttingRecipe.output;
    }

    private int GetOutputProcessForInput(KitchenObjectSO input)
    {
        CuttingRecipeSO outputCuttingRecipe = null;
        outputCuttingRecipe = GetCuttingRecipeForInput(input);
        return outputCuttingRecipe.processNumber;
    }

    public CuttingRecipeSO[] GetCuttingRecipeSOArray()
    {
        return cuttingRecipeSOArray;
    }

}
