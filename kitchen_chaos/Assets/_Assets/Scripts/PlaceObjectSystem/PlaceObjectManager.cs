using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceObjectManager : Singleton<PlaceObjectManager>
{
    private const float SIZE_OF_GRID = 2f;

    [SerializeField] private IPlaceable _placingObject;

    private bool _canPlace = false;
    public IPlaceable PlacingObject
    {
        get => _placingObject;
        set => _placingObject = value;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && _canPlace)
        {
            PlaceObject(_placingObject.Transform.position);
        }
        if (_placingObject != null && GameInput.Instance.GetMovementVectorNormalize() != Vector2.zero)
        {
            Vector3 testPos = Player.Instance.transform.position + GameInput.Instance.GetMovementVectorNormalize().ToVector3XZ() * 2;
            Vector3 gridPosition = GetGridPosition(testPos);
            _placingObject.Transform.position = gridPosition;
            _canPlace = true;
        }
    }

    public Vector3 GetGridPosition(Vector3 position)
    {
        var x = Mathf.RoundToInt(position.x / SIZE_OF_GRID);
        var y = Mathf.RoundToInt(position.z / SIZE_OF_GRID);
        return new Vector3(x * SIZE_OF_GRID, 0, y * SIZE_OF_GRID);
    }

    public void StartPlacingObject(IPlaceable objectToPlace)
    {
        Debug.Log("StartPlacingObject");
        _placingObject = objectToPlace;
    }

    public void PlaceObject(Vector3 position)
    {
        _placingObject.Transform.position = position;
        _placingObject.PlaceObject();
        PlacingObject = null;
        _canPlace = false;
    }
}
