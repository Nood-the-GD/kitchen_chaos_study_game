using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgressBar
{
    public event EventHandler<OnStateChangedEventArg> OnStateChanged;
    public event EventHandler<IHasProgressBar.OnProcessChangedEvenArgs> OnProcessChanged;

    public class OnStateChangedEventArg : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }

    [SerializeField] private FryingRecipeSO[] firingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

    private State currentState;
    private FryingRecipeSO fryingRecipeSO;
    private BurningRecipeSO burningRecipeSO;
    private float fryingTimer;
    private float burningTimer;
    private Player player;

    private void Start()
    {
        currentState = State.Idle;
    }

    private void Update()
    {



        switch(currentState)
        {
            case State.Idle:
                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = 1f
                });
                break;
            case State.Frying:
                fryingTimer += Time.deltaTime;

                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = fryingTimer / fryingRecipeSO.fryingTimerMax
                });

                if (fryingTimer >= fryingRecipeSO.fryingTimerMax)
                {
                    if(PhotonNetwork.IsMasterClient){
                        GetKitchenObject().DestroySelf();


                        KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);
                    }
                    
                    ChangeState(State.Fried);
                    burningTimer = 0;
                    burningRecipeSO = GetBurningRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                }
                break;
            case State.Fried:
                if(burningRecipeSO == null)
                {
                    burningRecipeSO = GetBurningRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                    return;
                }
                burningTimer += Time.deltaTime;

                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = burningTimer / burningRecipeSO.burningTimerMax
                });

                if (burningTimer >= burningRecipeSO.burningTimerMax)
                {

                    if(PhotonNetwork.IsMasterClient){
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);
                    }
                    ChangeState(State.Burned);

                        
                }
                break;
            case State.Burned:
                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = 1f
                });
                break;
        }
    }

    public override void Interact(Player player)
    {
        if (HasKitchenObject())
        {
            //Counter has kitchen object
            if (!player.HasKitchenObject())
            {
                //Player carrying nothing    
                //Move kitchen object to player
                GetKitchenObject().SetKitchenObjectParent(player);
                ChangeState(State.Idle);
            }
            else
            {
                KitchenObjectSO playerKitchenObjectSO = player.GetKitchenObject().GetKitchenObjectSO();
                //Player is carrying something
                if (player.GetKitchenObject() is CompleteDishKitchenObject)
                {
                    //Player is holding a set of kitchen object 
                    if (player.GetKitchenObject().TryGetCompleteDishKitchenObject(out CompleteDishKitchenObject completeDish))
                    {
                        if (completeDish.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO()))
                        {
                            GetKitchenObject().DestroySelf();
                            ChangeState(State.Idle);
                        }
                    }
                }
                else
                {
                    //Player is holding an ingredient
                    if(CompleteDishManager.Instance.TryCombineDish(playerKitchenObjectSO, GetKitchenObject().GetKitchenObjectSO(), out KitchenObjectSO resultDishSO))
                    {
                        player.GetKitchenObject().DestroySelf();

                        KitchenObjectSO counterKitchenObjectSO = GetKitchenObject().GetKitchenObjectSO();
                        KitchenObject.SpawnCompleteDish(resultDishSO, new KitchenObjectSO[] {playerKitchenObjectSO, counterKitchenObjectSO}, player);

                        GetKitchenObject().DestroySelf();
                        ChangeState(State.Idle);
                    }
                }
            }
        }
        else
        {
            //Counter don't have kitchen object
            if (player.HasKitchenObject() && HasFryingRecipeForInput(player.GetKitchenObject().GetKitchenObjectSO()))
            {
                //Player carrying something that can be fried
                //Move kitchen object to counter
                player.GetKitchenObject().SetKitchenObjectParent(this);
                fryingRecipeSO = GetFryingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                ChangeState(State.Frying);
                fryingTimer = 0;
            }
            //else
            //Player carrying nothing or something can not be cut
            //Do no thing
        }
    }


    private void ChangeState(State state)
    {
        currentState = state;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArg
        {
            state = currentState
        });
    }

    private FryingRecipeSO GetFryingRecipeWithInput(KitchenObjectSO input)
    {
        foreach (FryingRecipeSO recipe in firingRecipeSOArray)
        {
            if (recipe.input == input) return recipe;
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeWithInput(KitchenObjectSO input)
    {
        foreach (BurningRecipeSO recipe in burningRecipeSOArray)
        {
            if (recipe.input == input) return recipe;
        }
        return null;
    }

    private bool HasFryingRecipeForInput(KitchenObjectSO input)
    {
        return GetFryingRecipeWithInput(input) != null;
    }

    private KitchenObjectSO GetOutputForInput(KitchenObjectSO input)
    {
        FryingRecipeSO outputCuttingRecipe = GetFryingRecipeWithInput(input);
        return outputCuttingRecipe.output;
    }

    private int GetOutputFryingTimerForInput(KitchenObjectSO input)
    {
        FryingRecipeSO outputCuttingRecipe = null;
        outputCuttingRecipe = GetFryingRecipeWithInput(input);
        return outputCuttingRecipe.fryingTimerMax;
    }
}
