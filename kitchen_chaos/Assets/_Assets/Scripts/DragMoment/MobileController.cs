using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileController : MonoBehaviour
{
    [SerializeField]  private DragMovement _dragMovement;
    [SerializeField]  private Button _interactBtn, _useBtn;
    private Action _onInteract, _onUse;

    void Awake()
    {
        _interactBtn.onClick.AddListener(() =>
        {
            _onInteract?.Invoke();
        });
        _useBtn.onClick.AddListener(() =>
        {
            _onUse?.Invoke();
        });
    }

    void Update()
    {

    }

    public Vector2 GetPlayerMovementInput()
    {
        return _dragMovement.GetPlayerInput();
    }

    public void OnInteractBtnPress(Action onPress)
    {
        Debug.Log("Press Interaction");
        _onInteract = onPress;  
    }
    public void OnUseBtnPress(Action onPress)
    {
        _onUse = onPress;
    }
}
