using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgressBar
{
    public event EventHandler OnCutAction;
    public event EventHandler<IHasProgressBar.OnProcessChangedEvenArgs> OnProcessChanged;

    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

    private int cuttingProcess;

    public override void Interact(Player player)
    {
        if (HasKitchenObject())
        {
            //Counter has kitchen object
            if (!player.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(player);
            }
            //else
            //Player carrying something
            //Do nothing
        }
        else
        {
            //Counter don't have kitchen object
            if (player.HasKitchenObject() && HasRecipeForInput(player.GetKitchenObject().GetKitchenObjectSO()))
            {
                //Player carrying something that can be cut
                //Move kitchen object to counter
                player.GetKitchenObject().SetKitchenObjectParent(this);
                cuttingProcess = 0;

                int cuttingProgressNumber = GetOutputProcessForInput(GetKitchenObject().GetKitchenObjectSO());

                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = (float)cuttingProcess / cuttingProgressNumber
                });
            }
            //else
            //Player carrying nothing or something can not be cut
            //Do no thing
        }
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
            if(cuttingProcess >= cuttingProgressNumber)
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
