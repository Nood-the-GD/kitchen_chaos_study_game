using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : Singleton<CustomerManager>
{
    private Queue<Customer> _customerQueue = new Queue<Customer>();

    void OnEnable()
    {
        DeliveryManager.Instance.OnRecipeSuccess += DeliverFood;
    }

    private void DeliverFood(object sender, EventArgs e)
    {
        _customerQueue.Dequeue().GetFood();    
    }

    public void AddNewCustomer(Customer customer)
    {
        _customerQueue.Enqueue(customer);
    }
}
