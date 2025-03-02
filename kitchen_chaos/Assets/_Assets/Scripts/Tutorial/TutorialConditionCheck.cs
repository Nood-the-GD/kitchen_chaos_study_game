
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class TutorialConditionCheck : MonoBehaviour
{
    public List<Func<bool>> _conditions = new List<Func<bool>>();
    public List<KitchenObjectSO> _kitchenObjectList = new List<KitchenObjectSO>();

    void Awake()
    {
        Initialization();
    }

    private void Initialization()
    {
        AddCondition(CheckTomatoSlide);
        AddCondition(CheckCabbageSlices);
        AddCondition(CheckSalad);
        AddCondition(CheckChessBurger);
    }

    public void AddKitchenObjectToCheck(KitchenObjectSO kitchenObjectSO)
    {
        _kitchenObjectList.Add(kitchenObjectSO);
        CheckConditions();
    }

    public void AddCondition(Func<bool> condition)
    {
        _conditions.Add(condition);
    }

    private void CheckConditions()
    {
        for (int i = 0; i < _conditions.Count; i++)
        {
            if (_conditions[i]?.Invoke() == true)
            {
                _conditions.RemoveAt(i);
                i--;
            }
        }
    }

    #region Conditions
    private bool CheckTomatoSlide()
    {
        if (_kitchenObjectList.Contains(GameData.s.GetKitchenObjectSO("TomatoSlices")))
        {
            return true;
        }
        return false;
    }
    private bool CheckCabbageSlices()
    {
        if (_kitchenObjectList.Contains(GameData.s.GetKitchenObjectSO("CabbageSlices")))
        {
            return true;
        }
        return false;
    }
    private bool CheckSalad()
    {
        if (_kitchenObjectList.Contains(GameData.s.GetKitchenObjectSO("Salad")))
        {
            return true;
        }
        return false;
    }
    private bool CheckChessBurger()
    {
        if (_kitchenObjectList.Contains(GameData.s.GetKitchenObjectSO("CheeseBurger")))
        {
            return true;
        }
        return false;
    }
    #endregion
}