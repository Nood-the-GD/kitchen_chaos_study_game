using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonAnimation : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Transform _animateTransform;
    [SerializeField] private Vector3 _animateScale = new Vector3(-0.1f, -0.1f, -0.1f);
    private Button _button;
    private Vector3 _originalScale;

    void Awake()
    {
        _button = GetComponent<Button>();
        _originalScale = this.transform.localScale;
    }

    void OnValidate()
    {
        if (_animateTransform == null) _animateTransform = this.transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        if (_animateTransform != null)
        {
            _animateTransform.DOScale(this.transform.localScale + _animateScale, 0.1f).SetEase(Ease.OutQuad);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_button.interactable) return;

        if (_animateTransform != null)
        {
            _animateTransform.DOScale(_originalScale, 0.1f).SetEase(Ease.OutQuad);
        }
    }
}
