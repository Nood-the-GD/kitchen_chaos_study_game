using System.Collections.Generic;
using UnityEngine;

public class KitchenObjectSoManager : Singleton<KitchenObjectSoManager>
{
    [SerializeField] private List<KitchenObjectSO> _kitchenObjectSOs = new List<KitchenObjectSO>();

    public KitchenObjectSO GetRandomKitchenObjectSO()
    {
        return _kitchenObjectSOs[Random.Range(0, _kitchenObjectSOs.Count)];
    }
}
