using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridDebugDrawer : MonoBehaviour
{
    [SerializeField] private RestaurantGrid _restaurantGrid;
    [SerializeField] private Transform _gridPositionVisualPrefab;
    [SerializeField] private Material _chooseMat, _unChooseMat;
    private Transform[,] _visualTrans;
    private Transform selectedVisual;

    void Start()
    {
        _visualTrans = new Transform[_restaurantGrid.Width, _restaurantGrid.Height];
        _gridPositionVisualPrefab.gameObject.SetActive(false);
        foreach (var pos in _restaurantGrid.GridPositionArray)
        {
            Transform visual = Instantiate(_gridPositionVisualPrefab, pos.WorldPos, Quaternion.identity);
            visual.gameObject.SetActive(true);
            _visualTrans[pos.index.x, pos.index.y] = visual;
        }
    }

    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //     Vector3 mousePos = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : Vector3.zero;
        //     GridPosition gridPosition = _restaurantGrid.FindGridPosition(mousePos);
        //     Transform visual = _visualTrans[gridPosition.index.x, gridPosition.index.y];
        //     if (selectedVisual != null && selectedVisual != visual)
        //     {
        //         selectedVisual.gameObject.SetActive(true);
        //     }
        //     visual.gameObject.SetActive(false);
        //     selectedVisual = visual;
        // }
    }

    void OnDrawGizmos()
    {
        // draw grid
        for (int i = 0; i < _restaurantGrid.Width; i++)
        {
            for (int j = 0; j < _restaurantGrid.Height; j++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(new Vector3(i * _restaurantGrid.SizeOfGrid, 0, j * _restaurantGrid.SizeOfGrid), new Vector3(_restaurantGrid.SizeOfGrid, 0, _restaurantGrid.SizeOfGrid));
            }
        }
    }
}
