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
    private Recipe _curRecipe;
    private State _currentState;
    private float _fryingTimer;
    private float _burningTimer;
    #endregion

    #region Unity functions
    private void Start()
    {
        _currentState = State.Idle;
    }
    private void Update()
    {
        switch (_currentState)
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
                    processNormalize = _fryingTimer / _curRecipe.step
                });

                if (_fryingTimer >= _curRecipe.step)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpawnKitchenObject(_curRecipe.overCookOutput, this);
                    }

                    ChangeState(State.Fried);
                    _burningTimer = 0;
                    _curRecipe = GetKitchenObjectSO().GetFriedOnlyRecipe();
                }
                break;
            case State.Fried:
                if (_curRecipe == null)
                {

                    return;
                }
                _burningTimer += Time.deltaTime;

                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = _burningTimer / _curRecipe.step
                });

                if (_burningTimer >= _curRecipe.step)
                {

                    if (SectionData.s.isSinglePlay || PhotonNetwork.IsMasterClient)
                    {
                        GetKitchenObject().DestroySelf();

                        KitchenObject.SpawnKitchenObject(_curRecipe.overCookOutput, this);
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
    public override void Interact(IContainable player)
    {
        if (!HasKitchenObject())
        {
            //Counter don't have kitchen object
            if (player.HasKitchenObject() && GetKitchenObjectSO().CanFried())
            {
                //Player carrying something that can be fried
                //Move kitchen object to counter
                player.GetKitchenObject().SetContainerParent(this);
                _curRecipe = GetKitchenObjectSO().GetFriedOnlyRecipe();
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


    private void ChangeState(State state)
    {
        _currentState = state;
        OnStateChanged?.Invoke(this, new OnStateChangedEventArg
        {
            state = _currentState
        });
    }

}
