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
            await UniTask.Delay(1000); // Delay 1 second
            if (_waitingLine.NextCustomerGroupNumber == 0) continue;
            if (TableManager.s.GetAvailableTableModels(_waitingLine.NextCustomerGroupNumber).Count > 0)
                NextCustomerGroup();
        }
    }

    private void NextCustomerGroup()
    {
        CustomerGroup customerGroup = _waitingLine.GetNextCustomerGroup();
        TableModel tableModel = TableManager.s.GetAvailableTableModels(customerGroup.Number).FirstOrDefault();
        tableModel.IsAvailable = false;

        for (int i = 0; i < customerGroup.Number; i++)
        {
            Customer customer = customerGroup.Customers[i];
            customer.SetChair(tableModel.ChairsPositions[i], tableModel.ChairsRotations[i], i);
            customer.SetTable(tableModel.TableIndex);
        }
    }
}
