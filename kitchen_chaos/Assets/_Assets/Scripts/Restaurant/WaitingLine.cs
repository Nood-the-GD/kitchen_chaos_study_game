using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaitingLine : Singleton<WaitingLine>
{
    [SerializeField] private Transform _headLine;
    private Queue<CustomerGroup> _customerGroups = new Queue<CustomerGroup>();
    private int _totalCustomerInline = 0;
    public int NextCustomerGroupNumber = 0;

    public void AddCustomerGroup(CustomerGroup customerGroup)
    {
        _customerGroups.Enqueue(customerGroup);
        NextCustomerGroupNumber = _customerGroups.Count > 0 ? _customerGroups.ToArray()[0].Number : 0;
        SetCustomerInLine(customerGroup);
    }
    public CustomerGroup GetNextCustomerGroup()
    {
        CustomerGroup customerGroup = _customerGroups.Dequeue();
        NextCustomerGroupNumber = _customerGroups.Count > 0 ? _customerGroups.ToArray()[0].Number : 0;
        ReArrangeCustomers();
        return customerGroup;
    }

    public void SetCustomerInLine(CustomerGroup customerGroup)
    {
        foreach (Customer customer in customerGroup.Customers)
        {
            customer.AddTargetPosition(GetNextPositionInLine(), this.transform.rotation.eulerAngles);
            _totalCustomerInline++;
        }
    }

    public Vector3 GetNextPositionInLine()
    {
        return _headLine.position + (-_headLine.forward) * (_totalCustomerInline * 3f);
    }

    private void ReArrangeCustomers()
    {
        _totalCustomerInline = 0;
        foreach (CustomerGroup customerGroup in _customerGroups)
        {
            foreach (Customer customer in customerGroup.Customers)
            {
                customer.AddTargetPosition(GetNextPositionInLine(), this.transform.rotation.eulerAngles);
                _totalCustomerInline++;
            }
        }
    }
}
