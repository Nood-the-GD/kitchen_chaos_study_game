using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter
{
    [SerializeField] private CuttingRecipeSO[] cuttingRecipeSOArray;

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
            KitchenObjectSO outputKitchenObject = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

            //Destroy input kitchenObject.
            GetKitchenObject().DestroySelf();

            //Spawn output kitchenObject
            KitchenObject.SpawnKitchenObject(outputKitchenObject, this);
        }
        //else
        //There is a kitchenObject on this counter but can not be cut
        //Do nothing
    }

    private bool HasRecipeForInput(KitchenObjectSO input)
    {
        foreach(CuttingRecipeSO cuttingRecipe in cuttingRecipeSOArray)
        {
            if (cuttingRecipe.input == input) return true;
        }
        return false;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO input)
    {
        foreach(CuttingRecipeSO recipe in cuttingRecipeSOArray)
        {
            if (recipe.input == input) return recipe.output;
        }
        return null;
    }

}
