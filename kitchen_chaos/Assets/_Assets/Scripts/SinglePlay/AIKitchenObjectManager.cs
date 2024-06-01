using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using UnityEngine;

public class AIKitchenObjectManager : Singleton<AIKitchenObjectManager>
{
    public KitchenObjectSO GetKitchenObjectSoOriginal(KitchenObjectSO kitchenObjectSO)
    {
        if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedChop)
        {
            CuttingCounter cuttingCounter = AICounterManager.s.GetCuttingCounter();
            KitchenObjectSO original = cuttingCounter.GetCuttingInputForOutput(kitchenObjectSO);
            return original;
        }
        else if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedFried)
        {
            StoveCounter cuttingCounter = AICounterManager.s.GetStoveCounter();
            KitchenObjectSO original = cuttingCounter.GetFryingInputFromOutput(kitchenObjectSO);
            return original;
        }
        return kitchenObjectSO;
    }
    public bool IsNeedChop(KitchenObjectSO kitchenObjectSO)
    {
        CuttingCounter cuttingCounter = AICounterManager.s.GetCuttingCounter();

        foreach(var recipe in cuttingCounter.GetCuttingRecipeSOArray())
        {
            if(recipe.input == kitchenObjectSO)
            {
                return true;
            }
        }
        return false;
    }
    public bool IsNeedFry(KitchenObjectSO kitchenObjectSO)
    {
        StoveCounter stoveCounter = AICounterManager.s.GetStoveCounter();

        foreach(var recipe in stoveCounter.GetFryingRecipeSOArray())
        {
            if(recipe.input = kitchenObjectSO)
            {
                return true;
            }
        }
        return false;
    }
}
