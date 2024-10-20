using System;

public class CustomerManager : Singleton<CustomerManager>
{
    private Customer[] _customers = new Customer[1];

    public void ServeCustomer(int customerId, KitchenObject kitchenObject)
    {
        GetCustomer(customerId).Serve(kitchenObject);
    }

    public void AddCustomer(Customer customer)
    {
        int index = GetEmptyCustomerId();
        _customers[index] = customer;
    }

    public void RemoveCustomer(Customer customer)
    {
        int index = GetCustomerId(customer);
        _customers[index] = null;
    }

    private int GetEmptyCustomerId()
    {
        for (int i = 0; i < _customers.Length; i++)
        {
            if (_customers[i] == null)
                return i;
        }
        // Extend the array by 1
        Array.Resize(ref _customers, _customers.Length + 1);

        // Return the last index
        return _customers.Length - 1;
    }

    public int GetCustomerId(Customer customer)
    {
        return Array.IndexOf(_customers, customer);
    }

    public Customer GetCustomer(int customerId)
    {
        for (int i = 0; i < _customers.Length; i++)
        {
            if (_customers[i] != null && GetCustomerId(_customers[i]) == customerId)
                return _customers[i];
        }
        return null;
    }
}
