using System.Collections.Generic;
using UnityEngine;

public class WaitingLine : Singleton<WaitingLine>
{
    [SerializeField] private Transform _headLine;
    private Queue<CustomerGroup> _customerGroups = new Queue<CustomerGroup>();
    public int NextCustomerGroupNumber = 0;

    public void AddCustomerGroup(CustomerGroup customerGroup)
    {
        _customerGroups.Enqueue(customerGroup);
        NextCustomerGroupNumber = _customerGroups.Count > 0 ? _customerGroups.ToArray()[0].Number : 0;
    }
    public CustomerGroup GetNextCustomerGroup()
    {
        CustomerGroup customerGroup = _customerGroups.Dequeue();
        NextCustomerGroupNumber = _customerGroups.Count > 0 ? _customerGroups.ToArray()[0].Number : 0;
        return customerGroup;
    }
}
