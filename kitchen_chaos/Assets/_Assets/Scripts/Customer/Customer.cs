using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private enum CustomerState
    {
        Entering,
        Ordering,
        Eating,
        Leaving
    }

    #region Variables
    [SerializeField] private float _timeToOrder = 3f;
    [SerializeField] private float _timeToEat = 3f;
    [SerializeField] private float _speed = 4f;

    public PhotonView photonView;
    public KitchenObjectSO KitchenObjectSo => _kitchenObjectSo;

    private bool _isBlock;
    private CustomerAnimator _animator;
    private int _tableIndex;
    private Vector3 _chairPos;
    private Quaternion _chairRot;
    private KitchenObjectSO _kitchenObjectSo;
    private KitchenObject _kitchenObject;
    private int _chairIndex;
    private Vector3 _targetPositions;
    private Vector3 _targetRotation;
    private CancellationTokenSource _moveToken;
    #endregion

    #region Unity Functions
    void Awake()
    {
        _animator = GetComponentInChildren<CustomerAnimator>();
        photonView = GetComponent<PhotonView>();
        _moveToken = new CancellationTokenSource();
    }
    #endregion

    #region Customer Functions
    public void SetChair(Vector3 chairPosition, Quaternion chairRotation, int chairIndex)
    {
        _chairPos = chairPosition;
        _chairRot = chairRotation;
        _chairIndex = chairIndex;
        Debug.Log("SetChair " + "Customer: " + CustomerManager.s.GetCustomerId(this) + " Chair: " + _chairIndex);

        SetState(CustomerState.Entering);
    }
    public void SetTable(int tableIndex)
    {
        _tableIndex = tableIndex;
    }
    public void Serve(KitchenObject kitchenObject)
    {
        _kitchenObject = kitchenObject;
        SetState(CustomerState.Eating);
    }

    private void SetState(CustomerState state)
    {
        switch (state)
        {
            case CustomerState.Entering:
                WalkToTable();
                break;
            case CustomerState.Ordering:
                Order();
                break;
            case CustomerState.Eating:
                Eat();
                break;
            case CustomerState.Leaving:
                Leave();
                break;
        }
    }
    private async void WalkToTable()
    {
        _targetPositions = _chairPos;
        _targetRotation = _chairRot.eulerAngles;
        _moveToken.Cancel();
        _moveToken = new CancellationTokenSource();
        await MoveToTargetAsync(_moveToken.Token);

        // Customer has reached the table
        transform.rotation = _chairRot;

        SetState(CustomerState.Ordering);
    }
    private void Stop()
    {
        Debug.Log("Stop");
        _animator.Stop();
    }
    private void Rotate(Vector3 direction)
    {
        // Rotate to walking direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
    private async void Order()
    {
        _animator.PreOrder();
        await UniTask.WaitForSeconds(_timeToOrder);
        _kitchenObjectSo = DeliveryManager.Instance.OrderList[0];
        _animator.Order(_kitchenObjectSo);

        TableModel tableModel = TableManager.s.GetTableModel(_tableIndex);
        tableModel.AddCustomer(CustomerManager.s.GetCustomerId(this), _chairIndex);
        tableModel.AddOrder(_kitchenObjectSo.objectName, CustomerManager.s.GetCustomerId(this));
    }
    private async void Eat()
    {
        _animator.Eat();
        await UniTask.WaitForSeconds(_timeToEat);
        SetState(CustomerState.Leaving);
    }
    private async void Leave()
    {
        Destroy(_kitchenObject.gameObject);
        _targetPositions = CustomerSpawner.s.transform.position;
        await MoveToTargetAsync(_moveToken.Token);
        Destroy(this.gameObject);
    }
    #endregion

    #region Support
    #endregion

    #region Move
    private async UniTask MoveToTargetAsync(CancellationToken token)
    {
        bool isArrived = Vector3.Distance(transform.position, _targetPositions) < 0.1f;
        _animator.Walk();
        while (!isArrived)
        {
            try
            {
                if (this.transform == null) break;
                isArrived = Vector3.Distance(transform.position, _targetPositions) < 0.1f;

                await UniTask.WaitForEndOfFrame(this, token);
                Vector3 direction = (_targetPositions - transform.position).normalized;
                transform.position += direction * Time.deltaTime * _speed;
                Rotate(direction);
            }
            catch (OperationCanceledException e)
            {
                return;
            }
        }
        Stop();
        transform.SetPositionAndRotation(_targetPositions, Quaternion.Euler(_targetRotation));
    }
    public void AddTargetPosition(Vector3 targetPosition, Vector3 rotation)
    {
        _targetPositions = targetPosition;
        _targetRotation = rotation;
        MoveToTargetAsync(_moveToken.Token);
    }
    #endregion
}













