using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class StoveCounter : BaseCounter, IHasProgressBar
{
    #region Events
    public event EventHandler<OnStateChangedEventArg> OnStateChanged;
    public event EventHandler<IHasProgressBar.OnProcessChangedEvenArgs> OnProcessChanged;
    public class OnStateChangedEventArg : EventArgs
    {
        public State state;
    }
    #endregion

    #region Enum
    public enum State
    {
        Idle,
        Frying,
        Fried,
        Burned,
    }
    #endregion

    #region Variables
    [SerializeField] private FryingRecipeSO[] _firingRecipeSOArray;
    [SerializeField] private BurningRecipeSO[] _burningRecipeSOArray;

    private State _currentState;
    private FryingRecipeSO _fryingRecipeSO;
    private BurningRecipeSO _burningRecipeSO;
    private float _fryingTimer;
    private float _burningTimer;
    private Player _player;
    #endregion

    #region Unity functions
    private void Start()
    {
        _currentState = State.Idle;
    }
    private void Update()
    {
        switch(_currentState)
        {
            case State.Idle:
                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = 1f
                });
                break;
            case State.Frying:
                _fryingTimer += Time.deltaTime;

                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = _fryingTimer / _fryingRecipeSO.fryingTimerMax
                });

                if (_fryingTimer >= _fryingRecipeSO.fryingTimerMax)
                {
                    if(PhotonNetwork.IsMasterClient){
                        GetKitchenObject().DestroySelf();


                        KitchenObject.SpawnKitchenObject(_fryingRecipeSO.output, this);
                    }
                    
                    ChangeState(State.Fried);
                    _burningTimer = 0;
                    _burningRecipeSO = GetBurningRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                }
                break;
            case State.Fried:
                if(_burningRecipeSO == null)
                {
                    _burningRecipeSO = GetBurningRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                    return;
                }
                _burningTimer += Time.deltaTime;

                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = _burningTimer / _burningRecipeSO.burningTimerMax
                });

                if (_burningTimer >= _burningRecipeSO.burningTimerMax)
                {

                    if(PhotonNetwork.IsMasterClient){
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(_burningRecipeSO.output, this);
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
    #endregion

    #region Interact
    public override void Interact(IKitchenObjectParent player)
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
                _fryingRecipeSO = GetFryingRecipeWithInput(GetKitchenObject().GetKitchenObjectSO());
                ChangeState(State.Frying);
                _fryingTimer = 0;
            }
            //else
            //Player carrying nothing or something can not be cut
            //Do no thing
        }
    }
    #endregion

    public bool IsCookComplete()
    {
        return _currentState == State.Fried;
    }

    public KitchenObjectSO GetFryingInputFromOutput(KitchenObjectSO output)
    {
        foreach (FryingRecipeSO recipe in _firingRecipeSOArray)
        {
            if (recipe.output == output) return recipe.input;
        }
        return null;
    }

    private void ChangeState(State state)
    {
        _currentState = state;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArg
        {
            state = _currentState
        });
    }

    private FryingRecipeSO GetFryingRecipeWithInput(KitchenObjectSO input)
    {
        foreach (FryingRecipeSO recipe in _firingRecipeSOArray)
        {
            if (recipe.input == input) return recipe;
        }
        return null;
    }

    private BurningRecipeSO GetBurningRecipeWithInput(KitchenObjectSO input)
    {
        foreach (BurningRecipeSO recipe in _burningRecipeSOArray)
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

    public FryingRecipeSO[] GetFryingRecipeSOArray()
    {
        return _firingRecipeSOArray;
    }
}
