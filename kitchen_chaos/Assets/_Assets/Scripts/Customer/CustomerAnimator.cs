using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerAnimator : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite _perOrderIcon;
    private bool _isAnimating;

    #region Unity Function
    void Start()
    {
        _canvas.gameObject.SetActive(false);
    }
    #endregion

    #region Animation
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
    public void PreOrder()
    {
        _canvas.gameObject.SetActive(true);
        _icon.sprite = _perOrderIcon;
    }
    public void Order(KitchenObjectSO kitchenObjectSo)
    {
        _icon.sprite = kitchenObjectSo.sprite;
        _canvas.gameObject.SetActive(true);
    }
    public void Eat()
    {
        _canvas.gameObject.SetActive(false);
        _anim.Play("Eat");
    }
    #endregion

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
