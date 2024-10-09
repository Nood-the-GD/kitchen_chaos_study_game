using System.Collections.Generic;
using UnityEngine;

public class CustomerGroup
{
    public int Number => Customers.Count;
    public List<Customer> Customers;

    public CustomerGroup()
    {
        Customers = new List<Customer>();
    }
    public CustomerGroup(List<Customer> customers)
    {
        Customers = customers;
    }

    public void AddToList(Customer customer)
    {
        Customers.Add(customer);
    }
    public void RemoveFromList(Customer customer)
    {
        if (Customers.Contains(customer))
            Customers.Remove(customer);
    }
}
