using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerAnimator : MonoBehaviour
{
    private Animator _anim;
    private bool _isAnimating;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void Walk()
    {
        if (_anim != null)
            _anim.Play("Walk");
    }
    public void Stop()
    {
        if (_isAnimating == false && _anim != null)
            _anim.Play("Idle_A");
    }
    public void DeliverFood(Action onComplete)
    {
        if (_anim == null) return;
        _anim.Play("Roll");
        _isAnimating = true;
        float duration = _anim.GetCurrentAnimatorStateInfo(0).length;
        Debug.Log(duration);
        StartCoroutine(EndDeliverFoodAnimation_CR(duration, onComplete));
    }
    IEnumerator EndDeliverFoodAnimation_CR(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
        _isAnimating = false;
        Destroy(this.gameObject, 4f);
    }
}
