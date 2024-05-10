using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static BasketPlayerControls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/Input Reader")]
public class InputReader : ScriptableObject, IBasketPlayerActions
{
    public event Action<bool> JumpEvent;
    public event Action<float> MoveEvent;

    private BasketPlayerControls controls;

    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new BasketPlayerControls();
            controls.BasketPlayer.SetCallbacks(this);
        }

        controls.BasketPlayer.Enable();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            JumpEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            JumpEvent?.Invoke(false);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<float>());
    }
}
