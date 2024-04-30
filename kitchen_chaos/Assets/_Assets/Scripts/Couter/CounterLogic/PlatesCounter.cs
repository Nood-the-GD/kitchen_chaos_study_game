using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawn;
    public event EventHandler OnPlateRemove;

    [SerializeReference] private KitchenObjectSO plateKitchenObjectSO;
    private float plateTimer = 0f;
    private float plateTimerMax = 8f;

    private int plateNumberMax = 4;
    private int plateNumber = 0;
    private Player player;

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

    public override void Interact(Player player)
    {
        if (plateNumber <= 0) return;

        if (!player.HasKitchenObject())
        {
            //Player is hold nothing
            //PlateCounter has plates
            plateNumber--;
            KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
            OnPlateRemove?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            KitchenObjectSO playerKitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
            //Player is holding something
            if(player.GetKitchenObject() is CompleteDishKitchenObject)
            {
                //Player is holding a set of kitchen object
                CompleteDishKitchenObject playerCompleteDish = player.GetKitchenObject() as CompleteDishKitchenObject;
                // Try add ingredient with the current kitchen object on counter
                if(playerCompleteDish.TryAddIngredient(plateKitchenObjectSO))
                {
                    plateNumber--;
                    OnPlateRemove?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                //Try combine ingredient with player
                if(CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, plateKitchenObjectSO, out KitchenObjectSO resultDishSO))
                {
                    player.GetKitchenObject().DestroySelf();

                    KitchenObject.SpawnCompleteDish(resultDishSO, new KitchenObjectSO[] {playerKitchenObjectSO, plateKitchenObjectSO}, player);

                    GetKitchenObject().DestroySelf();

                    plateNumber--;
                    OnPlateRemove?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
