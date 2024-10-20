using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestaurantGrid : Singleton<RestaurantGrid>
{
    [SerializeField] private int _width, _height;
    private const int SIZE_OF_GRID = 2;

    private GridPosition[,] _gridPositionArray;

    public int Width => _width;
    public int Height => _height;
    public int SizeOfGrid => SIZE_OF_GRID;
    public GridPosition[,] GridPositionArray => _gridPositionArray;


    protected override void Awake()
    {
        base.Awake();
        _gridPositionArray = new GridPosition[_width, _height];

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                _gridPositionArray[i, j] = new GridPosition(i, j, SIZE_OF_GRID);
            }
        }
    }

    // public GridPosition FindGridPosition(Vector3 worldPosition)
    // {
    //     Debug.Log(worldPosition);
    //     var x = Mathf.RoundToInt(worldPosition.x / SIZE_OF_GRID);
    //     var y = Mathf.RoundToInt(worldPosition.z / SIZE_OF_GRID);
    //     Debug.Log(x + ", " + y);
    //     // Ensure x and y are within the grid bounds
    //     x = Mathf.Clamp(x, 0, _width - 1);
    //     y = Mathf.Clamp(y, 0, _height - 1);

    //     // Calculate world position directly from x and y
    //     Vector3 worldPos = new Vector3(x * SIZE_OF_GRID, 0, y * SIZE_OF_GRID);

    //     return _gridPositionArray[x, y];
    // }

    public Vector3 FindWorldPositionInGrid(Vector3 position)
    {
        var x = Mathf.RoundToInt(position.x / SIZE_OF_GRID);
        var y = Mathf.RoundToInt(position.z / SIZE_OF_GRID);
        return new Vector3(x * SIZE_OF_GRID, 0, y * SIZE_OF_GRID);
    }

    public Vector2 FindGridPositionIndex(GridPosition gridPosition)
    {
        foreach (var gridPos in _gridPositionArray)
        {
            if (gridPos.Equals(gridPosition))
            {
                return gridPos.index;
            }
        }
        // no result
        return new Vector2(-1, -1);
    }
}
