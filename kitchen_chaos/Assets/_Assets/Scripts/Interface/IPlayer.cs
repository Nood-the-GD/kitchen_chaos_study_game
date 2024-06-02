using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IKitchenObjectParent
{
    public bool IsWalking();
    public bool IsHolding();
}
