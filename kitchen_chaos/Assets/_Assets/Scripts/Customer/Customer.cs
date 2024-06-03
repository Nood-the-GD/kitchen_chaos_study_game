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

    void Awake()
    {
        _animator = GetComponentInChildren<CustomerAnimator>();
        photonView = GetComponent<PhotonView>();
    }
    void Start()
    {
        CustomerManager.s.AddNewCustomer(this);    
    }
    void Update()
    {
        if(_isBlock == false || _isDelivered == true)
        {
            Walk();
        }
        else
        {
            Stop();
        }
    }

    private void Walk()
    {
        _animator.Walk();
        this.transform.position += this.transform.forward * 2f * Time.deltaTime;
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

    void OnTriggerEnter(Collider other)
    {
        _isBlock = true;    
    }
    void OnTriggerExit(Collider other)
    {
        _isBlock = false;        
    }
}
