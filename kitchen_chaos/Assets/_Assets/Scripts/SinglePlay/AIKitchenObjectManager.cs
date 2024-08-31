using UnityEngine;

public class AIKitchenObjectManager : Singleton<AIKitchenObjectManager>
{
    public KitchenObjectSO GetKitchenObjectSoOriginal(KitchenObjectSO kitchenObjectSO)
    {
        // if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedChop)
        // {
        //     CuttingCounter cuttingCounter = AICounterManager.s.GetCuttingCounter();
        //     KitchenObjectSO original = cuttingCounter.GetCuttingInputForOutput(kitchenObjectSO);
        //     return original;
        // }
        // else if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedFried)
        // {
        //     StoveCounter stoveCounter = AICounterManager.s.GetStoveCounter();
        //     KitchenObjectSO original = stoveCounter.GetFryingInputFromOutput(kitchenObjectSO);
        //     return original;
        // }
        // return kitchenObjectSO;

        return null;
    }
    public bool IsNeedChop(KitchenObjectSO kitchenObjectSO)
    {
        // CuttingCounter cuttingCounter = AICounterManager.s.GetCuttingCounter();

        // if (cuttingCounter == null) return false;

        // foreach(var recipe in cuttingCounter.GetCuttingRecipeSOArray())
        // {
        //     if(recipe.input == kitchenObjectSO)
        //     {
        //         return true;
        //     }
        // }

        Debug.LogError("Not implementd");
        return false;
    }
    public bool IsNeedFry(KitchenObjectSO kitchenObjectSO)
    {
        // StoveCounter stoveCounter = AICounterManager.s.GetStoveCounter();

        // if (stoveCounter == null) return false;

        // foreach (var recipe in stoveCounter.GetFryingRecipeSOArray())
        // {
        //     if (recipe.input == kitchenObjectSO)
        //     {
        //         return true;
        //     }
        // }
        Debug.LogError("Not implementd");
        return false;
    }
}
