using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public CompleteDishSO completeDish;
    public float timer;
    public float maxTimer;

}

[CreateAssetMenu(fileName = "CompleteDishSO")]
public class CompleteDishSO : KitchenObjectSO
{
    public float waitingTime = 5;

    public List<KitchenObjectSO> ingredients;
    public Order ConvertToOrder()
    {
        var order = new Order
        {
            completeDish = this,
            timer = waitingTime,
            maxTimer = waitingTime,
        };
        return order;
    }


}
