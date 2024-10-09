using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct GridPosition : IEquatable<GridPosition>
{
    public GridPosition(int x, int y, int size = 1)
    {
        this.x = x;
        this.y = y;
        this.size = size;
    }
    public int x;
    public int y;
    public int size;
    public Vector2Int index => new Vector2Int(x, y);
    public Vector3 WorldPos => new Vector3(x * size, 0, y * size);


    public bool Equals(GridPosition other)
    {
        return other.x == x && other.y == y;
    }
}
