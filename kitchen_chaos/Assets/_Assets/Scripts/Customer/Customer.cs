using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] private float _timeToOrder = 3f;
    [SerializeField] private float _timeToEat = 3f;

    public PhotonView photonView;
    public KitchenObjectSO KitchenObjectSo => _kitchenObjectSo;

    private bool _isBlock;
    private CustomerAnimator _animator;
    private Table _table;
    private Vector3 _chairPos;
    private Quaternion _chairRot;
    private float _speed = 4f;
    private KitchenObjectSO _kitchenObjectSo;
    private KitchenObject _kitchenObject;


    #region Unity Functions
    void Awake()
    {
        _animator = GetComponentInChildren<CustomerAnimator>();
        photonView = GetComponent<PhotonView>();
    }
    void Start()
    {
        // CustomerManager.s.AddNewCustomer(this);
    }
    void Update()
    {

    }
    #endregion

    #region Serve
    public void Serve(IKitchenObjectParent KOParent)
    {
        Vector3 servePosition = GetServePosition();
        KitchenObject kitchenObject = KOParent.GetKitchenObject();
        kitchenObject.SetKitchenObjectParent(_table);
        kitchenObject.transform.position = servePosition;
        _kitchenObject = kitchenObject;
        _table.RemoveCustomer(this);

        SetState(CustomerState.Eating);
    }
    #endregion

    #region Customer Functions
    public void SetChair(Vector3 chairPosition, Quaternion chairRotation)
    {
        _chairPos = chairPosition;
        _chairRot = chairRotation;

        SetState(CustomerState.Entering);
    }
    public void SetTable(Table table)
    {
        _table = table;
    }
    private void SetState(CustomerState state)
    {
        switch (state)
        {
            case CustomerState.Entering:
                Walk();
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
    private async void Walk()
    {
        Vector3 targetPosition = _chairPos;
        await MoveToTargetAsync(targetPosition);

        // Customer has reached the table
        transform.rotation = _chairRot;
        Stop();

        SetState(CustomerState.Ordering);
    }
    private void Stop()
    {
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
        _kitchenObjectSo = KitchenObjectSoManager.s.GetRandomKitchenObjectSO();
        _animator.Order(_kitchenObjectSo);
        _table.AddCustomer(this);
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
        Vector3 targetPosition = CustomerSpawner.s.transform.position;
        await MoveToTargetAsync(targetPosition);
        Destroy(this.gameObject);
    }
    #endregion

    #region Support
    private Vector3 GetServePosition()
    {
        Vector3 direction = (transform.position - _table.Transform.position).normalized;
        Vector3 servePosition = _table.Transform.position + direction * .5f;
        servePosition.y = 1f;
        return servePosition;
    }
    #endregion

    #region Move
    private async UniTask MoveToTargetAsync(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            await UniTask.WaitForEndOfFrame(this);
            _animator.Walk();
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * Time.deltaTime * _speed;
            Rotate(direction);
        }
    }
    #endregion

    #region Trigger
    void OnTriggerEnter(Collider other)
    {
        _isBlock = true;
    }
    void OnTriggerExit(Collider other)
    {
        _isBlock = false;
    }
    #endregion
}













