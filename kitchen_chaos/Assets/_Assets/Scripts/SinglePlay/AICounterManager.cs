using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AICounterManager : Singleton<AICounterManager>
{
    [SerializeField] private List<BaseCounter> _allCounters = new List<BaseCounter>();

    protected override void Start()
    {
        base.Start();
        _allCounters = FindObjectsOfType<BaseCounter>().ToList();
    }

    public bool TryGetCounterHasKitchenObject(KitchenObjectSO kitchenObjectSO, out BaseCounter resultCounter)
    {
        // foreach (var counter in _allCounters)
        // {
        //     if (counter.HasKitchenObject() && counter.GetKitchenObject().GetKitchenObjectSO() == kitchenObjectSO)
        //     {
        //         resultCounter = counter;
        //         return true;
        //     }
        // }
        // if (kitchenObjectSO.kitchenObjectType == KitchenObjectType.Original)
        // {
        //     resultCounter = GetContainerCounter(kitchenObjectSO);
        //     return true;
        // }
        // resultCounter = null;
        // Debug.LogError("This method is not implemented yet");
        resultCounter = null;
        return false;
    }

    public bool TryGetEmptyClearCounter(out BaseCounter resultCounter)
    {
        foreach (var counter in _allCounters)
        {
            if (!counter.HasKitchenObject() && counter is ClearCounter)
            {
                resultCounter = counter;
                return true;
            }
        }
        resultCounter = null;
        return false;
    }

    public bool TryGetEmptyCuttingCounter(out CuttingCounter cuttingCounter)
    {
        foreach (var counter in _allCounters)
        {
            if (counter is CuttingCounter && counter.HasKitchenObject() == false)
            {
                cuttingCounter = counter as CuttingCounter;
                return true;
            }
        }
        cuttingCounter = null;
        return false;
    }
    public bool TryGetEmptyStoveCounter(out StoveCounter stoveCounter)
    {
        foreach (var counter in _allCounters)
        {
            if (counter is StoveCounter && counter.HasKitchenObject() == false)
            {
                stoveCounter = counter as StoveCounter;
                return true;
            }
        }
        stoveCounter = null;
        return false;
    }

    public ContainerCounter GetContainerCounter(KitchenObjectSO kitchenObjectSO)
    {
        foreach (var counter in _allCounters)
        {
            if (counter is ContainerCounter)
            {
                ContainerCounter containerCounter = counter as ContainerCounter;
                if (containerCounter.GetContainerKitchenObject() == kitchenObjectSO)
                {
                    return counter as ContainerCounter;
                }
            }
        }
        return null;
    }
    public CuttingCounter GetCuttingCounter()
    {
        if (_allCounters.Any(x => x is CuttingCounter))
        {
            return _allCounters.First(x => x is CuttingCounter) as CuttingCounter;
        }
        return null;
    }
    public StoveCounter GetStoveCounter()
    {
        if (_allCounters.Any(x => x is StoveCounter))
        {
            return _allCounters.First(x => x is StoveCounter) as StoveCounter;
        }
        return null;
    }
    public PlatesCounter GetPlatesCounter()
    {
        if (_allCounters.Any(x => x is PlatesCounter))
        {
            return _allCounters.First(x => x is PlatesCounter) as PlatesCounter;
        }
        return null;
    }
    public DeliveryCounter GetDeliveryCounter()
    {
        if (_allCounters.Any(x => x is DeliveryCounter))
        {
            return _allCounters.First(x => x is DeliveryCounter) as DeliveryCounter;
        }
        return null;
    }
}
