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
        Complete
    }
    #endregion

    #region Variables
    private RecipeSO _curRecipe;
    private State _currentState;
    private float _fryingTimer;
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
                        KitchenObject.SpawnKitchenObject(_curRecipe.output, this);
                        CheckCanFried();
                    }
                }
                break;
            case State.Complete:
                OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
                {
                    processNormalize = 0f
                });
                break;
        }
    }
    #endregion

    #region Interact
    public override void Interact(IKitchenContainable player)
    {
        base.Interact(player);
        CmdAfterInteract();
    }
    #endregion

    private void CmdAfterInteract()
    {
        photonView.RPC(nameof(RPCAfterInteract), RpcTarget.All);
    }

    [PunRPC]
    private void RPCAfterInteract()
    {
        if (HasKitchenObject())
        {
            CheckCanFried();
        }
        else
        {
            OnProcessChanged?.Invoke(this, new IHasProgressBar.OnProcessChangedEvenArgs
            {
                processNormalize = 0f
            });
            _fryingTimer = 0;
            ChangeState(State.Idle);
        }
    }

    private void CheckCanFried()
    {
        var kitchenObjectSO = GetKitchenObjectSO();
        if (kitchenObjectSO.CanFried())
        {
            _curRecipe = CookingBookSO.s.GetFryingRecipe(kitchenObjectSO);
            ChangeState(State.Frying);
            _fryingTimer = 0;
        }
        else if (_currentState == State.Frying)
        {
            ChangeState(State.Complete);
        }
        else
        {
            ChangeState(State.Idle);
        }
    }

    public bool IsCookComplete()
    {
        return _currentState == State.Complete;
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
