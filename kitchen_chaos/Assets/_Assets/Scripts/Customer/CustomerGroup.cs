using System.Collections.Generic;
using UnityEngine;

public class CustomerGroup
{
    public int Number => Customers.Length;
    public Customer[] Customers;

    public CustomerGroup(int numberOfPeople)
    {
        Customers = new Customer[numberOfPeople];
    }
    public CustomerGroup(Customer[] customers)
    {
        Customers = customers;
    }

}
