using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MobileController : MonoBehaviour
{
    [SerializeField] private FloatingJoystick _floatingJoystick;
    [SerializeField] private Button _interactBtn, _useBtn;
    private Action _onInteract, _onUse, _onChangeCharacter;

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
        //TODO: Add change character button

        // _changeCharacterBtn.onClick.AddListener(() =>
        // {
        //     _onChangeCharacter?.Invoke();
        // });
    }

    void Update()
    {
    }

    public Vector2 GetPlayerMovementInput()
    {
        return _floatingJoystick.Direction;
    }

    public void OnInteractBtnPress(Action onPress)
    {
        _onInteract = onPress;
    }
    public void OnUseBtnPress(Action onPress)
    {
        _onUse = onPress;
    }
    public void OnChangeCharacterBtnPress(Action onPress)
    {
        _onChangeCharacter = onPress;
    }
}
