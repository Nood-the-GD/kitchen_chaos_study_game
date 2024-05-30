using System.Collections;
using System.Collections.Generic;
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
        return null;
    }
}
