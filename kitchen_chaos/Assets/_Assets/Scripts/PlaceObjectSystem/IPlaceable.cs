using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlaceable
{
    public Transform Transform { get; }
    void StartPlacing();
    void PlaceObject();
}
