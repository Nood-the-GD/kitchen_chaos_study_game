using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public PhotonView photonView;

    private bool _isBlock;
    private bool _isDelivered;
    private CustomerAnimator _animator;
    private Vector3 _chairPos;
    private Quaternion _chairRot;
    private bool _isHasChair;
    private float _speed = 2f;

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
        if (_chairPos != Vector3.zero)
        {
            Walk();
        }
    }
    #endregion

    #region Customer Functions
    private void Walk()
    {
        _animator.Walk();
        Vector3 targetPosition = _chairPos;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * Time.deltaTime * _speed;

        Rotate(direction);

        // Check if the customer has reached the table
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            _chairPos = Vector3.zero; // Clear the table reference once reached
            _isHasChair = false;
            transform.rotation = _chairRot;
            Stop();
        }
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
    public void SetChair(Vector3 chairPosition, Quaternion chairRotation)
    {
        _isHasChair = true;
        _chairPos = chairPosition;
        _chairRot = chairRotation;
    }
    private void Stop()
    {
        _animator.Stop();
    }
    public void GetFood()
    {
        _animator.DeliverFood(() =>
        {
            _isDelivered = true;
        });
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













