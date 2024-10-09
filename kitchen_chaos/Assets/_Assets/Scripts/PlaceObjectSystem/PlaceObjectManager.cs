using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceObjectManager : Singleton<PlaceObjectManager>
{
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
            Vector3 worldPosition = RestaurantGrid.s.FindWorldPositionInGrid(testPos);
            _placingObject.Transform.position = worldPosition;
            _canPlace = true;
        }
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
