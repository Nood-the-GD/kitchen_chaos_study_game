using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayer : IKitchenContainable
{
    public bool IsWalking();
    public bool IsHolding();
}
