using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgressBar
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
    private int cuttingProcess;
    private Player player;
    #endregion

    public override void Interact(Player player)
    {
        if(player == null){
            Debug.LogError("player is null");
            return;
        }

        if(HasKitchenObject())
        {
            //Counter has kitchen object
            if(!player.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            else
            {
                //Player is carrying something
                if(player.GetKitchenObject() is CompleteDishKitchenObject)
                {
                    //Player is holding a set of kitchen object
                    CompleteDishKitchenObject playerCompleteDish = player.GetKitchenObject() as CompleteDishKitchenObject;
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
                    this.player = player;
                    TryAddPlayerIngredient();
                }
            }
        }
        else
        {
            //Counter don't have kitchen object
            if(player.HasKitchenObject())
            {
                //Player carrying something
                //Move kitchen object to counter
                player.GetKitchenObject().SetKitchenObjectParent(this);
                if(HasRecipeForInput(GetKitchenObject().GetKitchenObjectSO()))
                {
                    cuttingProcess = 0;
                }
            }
            //else
            //Player carrying nothing
            //Do no thing
        }
    }

    private void TryAddPlayerIngredient()
    {
        KitchenObjectSO playerKitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
        if(GetKitchenObject() is CompleteDishKitchenObject)
        {
            // Kitchen object on counter is a set of kitchen object
            CompleteDishKitchenObject counterCompleteDish = GetKitchenObject() as CompleteDishKitchenObject;
            if(counterCompleteDish.TryAddIngredient(playerKitchenObjectSO))
            {
                player.GetKitchenObject().DestroySelf();
            }
        }
        else
        {
            // Kitchen object on counter is not a set of kitchen object
            // Try combine kitchen object on table and player
            if(CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
            {
                KitchenObject.OnAnyKitchenObjectSpawned += KitchenObject_OnAnyKitchenObjectSpawned;
                KitchenObject.SpawnKitchenObject(resultDishSO, null);
                KitchenObject.OnAnyKitchenObjectSpawned -= KitchenObject_OnAnyKitchenObjectSpawned;
            }
        }
    }

    private void KitchenObject_OnAnyKitchenObjectSpawned(KitchenObject completeDish)
    {
        KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();

        CompleteDishKitchenObject completeDishKitchenObject = completeDish as CompleteDishKitchenObject;
        completeDishKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO());
        completeDishKitchenObject.TryAddIngredient(counterKitchenObjectSO);

        player.GetKitchenObject().DestroySelf();
        GetKitchenObject().DestroySelf();
        completeDish.SetKitchenObjectParent(this);
        this.player = null;
    }

    public override void Chop(Player player)
    {
        if(HasKitchenObject() && HasRecipeForInput(GetKitchenObject().GetKitchenObjectSO()))
        {
            //There is a kitchenObject on this counter and it can be cut.
            //Get output kitchenObject base on input with recipe.
            cuttingProcess++;
            int cuttingProgressNumber = GetOutputProcessForInput(GetKitchenObject().GetKitchenObjectSO());
            OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
            {
                processNormalize = (float)cuttingProcess / cuttingProgressNumber
            });

            OnCutAction?.Invoke(this, EventArgs.Empty);
            OnCut?.Invoke(this, EventArgs.Empty);
            if(cuttingProcess >= cuttingProgressNumber && player.photonView.IsMine)
            {
                KitchenObjectSO outputKitchenObject = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

                //Destroy input kitchenObject.

                GetKitchenObject().DestroySelf();

                //Spawn output kitchenObject
                KitchenObject.SpawnKitchenObject(outputKitchenObject, this);
            }
        }
        //else
        //There is a kitchenObject on this counter but can not be cut
        //Do nothing
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

}
