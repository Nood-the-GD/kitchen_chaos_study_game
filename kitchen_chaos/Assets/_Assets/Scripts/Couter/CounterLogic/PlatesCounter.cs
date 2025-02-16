using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawn;
    public event EventHandler OnPlateRemove;
    public static event EventHandler OnAnyPlateRemove;
    private float plateTimer = 0f;
    private float plateTimerMax = 8f;

    private int plateNumberMax = 4;
    private int plateNumber = 0;

    private void Update()
    {
        plateTimer += Time.deltaTime;
        if(plateTimer >= plateTimerMax)
        {
            plateTimer = 0;

            if(plateNumber < plateNumberMax)
            {
                //Spawn new Plate on visual only
                OnPlateSpawn?.Invoke(this, EventArgs.Empty);
                plateNumber++;
            }
        }
    }


    public override void Interact(IKitchenContainable player)
    {
        if (plateNumber <= 0) return;

        if (!player.HasKitchenObject())
        {
            //Player is hold nothing
            plateNumber--;
            KitchenObject.SpawnKitchenObject(ObjectEnum.Plate, player);
            OnPlateRemove?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            KitchenObjectSO playerKitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
            //Player is holding something
            if(player.GetKitchenObject())
            {
                if(player.GetKitchenObject().TryAddPlate()){
                    plateNumber--;
                    OnPlateRemove?.Invoke(this, EventArgs.Empty);
                    OnAnyPlateRemove?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
