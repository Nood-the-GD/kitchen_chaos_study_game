using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableModel
{
    private int _numberOfChairs;
    public int TableIndex;
    public int NumberOfChairs
    {
        get { return _numberOfChairs; }
        set
        {
            _numberOfChairs = value;
            CustomerIds = new int[_numberOfChairs];
            Orders = new string[_numberOfChairs];
        }
    }
    public Vector3 TablePosition;
    public List<Vector3> ChairsPositions;
    public List<Quaternion> ChairsRotations;
    public int[] CustomerIds;
    public string[] Orders;
    public bool IsAvailable;

    public void Serve(KitchenObject kitchenObject)
    {
        int orderIndex = Array.IndexOf(Orders, kitchenObject.GetKitchenObjectSO().objectName);
        Debug.Log("Serve: " + orderIndex);
        Vector3 servePosition = GetServePosition(orderIndex);
        servePosition.y = 1f;
        kitchenObject.transform.position = servePosition;
        CustomerManager.s.ServeCustomer(CustomerIds[orderIndex], kitchenObject);
        Orders[orderIndex] = null;
    }

    public bool CanServe(KitchenObjectSO kitchenObjectSO)
    {
        Debug.Log("CanServe: " + kitchenObjectSO.objectName);
        return Orders.Any(order => order != null && order.Equals(kitchenObjectSO.objectName));
    }

    private Vector3 GetServePosition(int chairIndex)
    {
        Debug.Log("ChairsPositions: " + ChairsPositions[chairIndex] + " index: " + chairIndex + " ");
        return TablePosition + (ChairsPositions[chairIndex] - TablePosition).normalized * .5f;
    }

    public void AddCustomer(int customerId, int chairIndex)
    {
        CustomerIds[chairIndex] = customerId;
    }

    public void ClearTable()
    {
        CustomerIds = new int[NumberOfChairs];
        Orders = new string[NumberOfChairs];
        IsAvailable = true;
    }

    public void AddOrder(string orderName, int customerId)
    {
        int index = Array.IndexOf(CustomerIds, customerId);
        Orders[index] = orderName;
    }
}
