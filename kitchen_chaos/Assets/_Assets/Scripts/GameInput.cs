using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnUseAction;
    public event EventHandler OnPauseAction;

    private PlayerInputAction playerInputAction;
    [SerializeField] private MobileController mobileController;

    private void Awake()
    {
        Instance = this;

        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();

        playerInputAction.Player.Interaction.performed += Interaction_performed;
        playerInputAction.Player.Chop.performed += Chop_performed;
        playerInputAction.Player.Pause.performed += Pause_performed;

    }

    void Start()
    {
        mobileController?.OnInteractBtnPress(() => OnInteractAction?.Invoke(this, EventArgs.Empty));
        mobileController?.OnUseBtnPress(() => OnUseAction?.Invoke(this, EventArgs.Empty));
    }

    private void Update()
    {
    }

    private void Pause_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Chop_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnUseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interaction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalize()
    {
        Vector2 inputVector = Vector2.zero;
#if UNITY_EDITOR
        if (MovementTypeController.Instance.isMobileController)
        {
            inputVector = mobileController.GetPlayerMovementInput();
        }
        else
        {
            inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();
        }
        return inputVector.normalized;
#elif UNITY_STANDALONE // for pc
        inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();
        return inputVector.normalized;
#elif UNITY_ANDROID || UNITY_IOS
        inputVector = mobileController.GetPlayerMovementInput();
        return inputVector.normalized;
#endif
    }
}
