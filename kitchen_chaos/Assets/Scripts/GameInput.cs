using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnInteractAction;
    private PlayerInputAction playerInputAction;

    private void Awake()
    {
        playerInputAction = new PlayerInputAction();
        playerInputAction.Player.Enable();

        playerInputAction.Player.Interaction.performed += Interaction_performed;
    }

    private void Interaction_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        
    }

    public Vector2 MovementVectorNormalize()
    {
        Vector2 inputVector = playerInputAction.Player.Move.ReadValue<Vector2>();

        return inputVector.normalized;
    }
}
