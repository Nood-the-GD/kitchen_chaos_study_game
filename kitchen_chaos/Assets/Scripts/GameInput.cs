using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;
    public event EventHandler OnChopAction;
    private PlayerInputAction playerInputAction;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();

        playerInputAction.Player.Interaction.performed += Interaction_performed;
        playerInputAction.Player.Chop.performed += Chop_performed;
    }

    private void Chop_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnChopAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interaction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 MovementVectorNormalize()
    {
        Vector2 inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();

        return inputVector.normalized;
    }
}
