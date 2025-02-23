using System;
using UnityEngine;
using UnityEngine.UI;

public class MobileController : MonoBehaviour
{
    [SerializeField] private FloatingJoystick _floatingJoystick;
    [SerializeField] private Button _interactBtn, _useBtn, _changeCharacterBtn;
    private Action _onInteract, _onUse, _onChangeCharacter;

    void Awake()
    {
        Player.OnSelectedCounterChanged += Player_OnSelectedCounterChanged;

        _interactBtn.onClick.AddListener(() =>
        {
            _onInteract?.Invoke();
        });
        _useBtn.onClick.AddListener(() =>
        {
            _onUse?.Invoke();
        });

        if (SectionData.s.isSinglePlay)
        {
            _changeCharacterBtn.gameObject.SetActive(true);
            _changeCharacterBtn.onClick.AddListener(() =>
            {
                _onChangeCharacter?.Invoke();
            });
        }
        else
        {
            _changeCharacterBtn.gameObject.SetActive(false);
        }
    }
    void OnDestroy()
    {
        Player.OnSelectedCounterChanged -= Player_OnSelectedCounterChanged;
    }

    private void Player_OnSelectedCounterChanged(object sender, Player.OnSelectedCounterChangedEventArgs e)
    {
        if (e.selectedCounter is CuttingCounter)
        {
            _useBtn.gameObject.SetActive(true);
        }
        else
        {
            _useBtn.gameObject.SetActive(false);
        }

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
