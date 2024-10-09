using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ServingManager : Singleton<ServingManager>
{
    [SerializeField] private WaitingLine _waitingLine;

    protected override void Start()
    {
        base.Start();
        DelayLoop();
    }

    private async void DelayLoop()
    {
        while (true)
        {
            await UniTask.Delay(2000); // Delay 2 seconds
            if (_waitingLine.NextCustomerGroupNumber == 0) continue;
            if (TableManager.s.GetAvailableTableModels(_waitingLine.NextCustomerGroupNumber).Count > 0)
                NextCustomerGroup();
        }
    }

    private void NextCustomerGroup()
    {
        CustomerGroup customerGroup = _waitingLine.GetCustomerGroup();
        TableModel tableModel = TableManager.s.GetAvailableTableModels(customerGroup.Number).FirstOrDefault();

        for (int i = 0; i < customerGroup.Number; i++)
        {
            Customer customer = customerGroup.Customers[i];
            customer.SetChair(tableModel.ChairsPosition[i], tableModel.ChairsRotation[i]);
        }

        tableModel.IsAvailable = false;
    }
}
