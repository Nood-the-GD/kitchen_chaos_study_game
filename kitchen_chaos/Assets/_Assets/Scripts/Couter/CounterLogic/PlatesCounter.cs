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
    private float plateTimerMax = 4f;

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
        if (!player.HasKitchenObject())
        {
            //Player is hold nothing
            if(plateNumber > 0)
            {
                //PlateCounter has plates
                plateNumber--;
                KitchenObject.SpawnKitchenObject(plateKitchenObjectSO, player);
                OnPlateRemove?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            //Player is holding something
            if(player.GetKitchenObject() is CompleteDishKitchenObject)
            {
                //Player is holding a set of kitchen object
                CompleteDishKitchenObject playerCompleteDish = player.GetKitchenObject() as CompleteDishKitchenObject;
                // Try add ingredient with the current kitchen object on counter
                playerCompleteDish.TryAddIngredient(plateKitchenObjectSO);
            }
            else
            {
                //Try combine ingredient with player
                if(CompleteDishManager.Instance.TryCombineDish(player.GetKitchenObject().GetKitchenObjectSO(), plateKitchenObjectSO, out KitchenObjectSO resultDishSO))
                {
                    //This dish need a plate
                    this.player = player;
                    KitchenObject.OnAnyKitchenObjectSpawned += KitchenObject_OnAnyKitchenObjectSpawned;
                    KitchenObject.SpawnKitchenObject(resultDishSO, null);
                    KitchenObject.OnAnyKitchenObjectSpawned -= KitchenObject_OnAnyKitchenObjectSpawned;
                }
            }
        }
    }

    private void KitchenObject_OnAnyKitchenObjectSpawned(KitchenObject completeDish)
    {
        if(completeDish is not CompleteDishKitchenObject) return;

        CompleteDishKitchenObject completeDishKitchenObject = completeDish as CompleteDishKitchenObject;
        completeDishKitchenObject.TryAddIngredient(player.GetKitchenObject().GetKitchenObjectSO());
        completeDishKitchenObject.TryAddIngredient(plateKitchenObjectSO);

        player.GetKitchenObject().DestroySelf();
        completeDish.SetKitchenObjectParent(player);
        this.player = null;
    }
}
