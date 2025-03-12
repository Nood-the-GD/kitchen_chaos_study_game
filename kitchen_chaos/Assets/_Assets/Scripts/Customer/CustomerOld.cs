using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class CustomerOld : MonoBehaviour
{
    private PhotonView _photonView;
    private CustomerAnimator _customerAnimator;
    private float _speed = 3f;
    private bool _isBlocked = false;
    private bool _forceMove => CustomerSpawner.s.IsTopList(this);
    private bool _isLeaving = false;

    void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _customerAnimator = GetComponentInChildren<CustomerAnimator>();
    }
    void OnTriggerEnter(Collider other)
    {
        _isBlocked = true;
    }

    void OnTriggerExit(Collider other)
    {
        _isBlocked = false;
    }

    private async void OnRecipeSuccess(object sender, EventArgs e)
    {
        if (CustomerSpawner.s.IsTopList(this))
        {
            CustomerSpawner.s.RemoveCustomerOld(this);
            await Eat();
            Leave();
        }
    }

    public async UniTask Move(Vector3 position)
    {
        var distance = Vector3.Distance(transform.position, position);
        while (distance > 0.1f)
        {
            if (_isBlocked && _forceMove == false && _isLeaving == false)
            {
                await UniTask.DelayFrame(10);
                _customerAnimator.Stop();
                continue;
            }
            _customerAnimator.Walk();
            Vector3 dir = (position - transform.position).normalized;
            transform.position += dir * Time.deltaTime * _speed;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * _speed);
            distance = Vector3.Distance(transform.position, position);
            await UniTask.DelayFrame(1);
        }
        if (PhotonNetwork.IsMasterClient)
            DeliveryManager.Instance.OnRecipeSuccess += OnRecipeSuccess;
        _customerAnimator.Stop();
    }

    public async UniTask Eat()
    {
        _customerAnimator.Roll();
        await UniTask.WaitForSeconds(_customerAnimator.Animator.GetAnimationLength("Roll"));
    }

    private async UniTask Leave()
    {
        _isLeaving = true;
        _customerAnimator.Walk();
        DeliveryManager.Instance.OnRecipeSuccess -= OnRecipeSuccess;
        await Move(this.transform.position - new Vector3(0, 0, 20f));
        CmdDestroy();
    }

    private void CmdDestroy()
    {
        _photonView.RPC(nameof(RpcDestroy), RpcTarget.All);
    }
    [PunRPC]
    private void RpcDestroy()
    {
        Destroy(this.gameObject);
    }
}
