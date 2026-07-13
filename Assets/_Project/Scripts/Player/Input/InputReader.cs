using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, Controls.IPlayerActions
{
    public Vector2 MouseDelta { get; private set; }
    public Vector2 MoveComposite { get; private set; }
    public float MovementInputDuration { get; set; }
    public bool MovementInputDetected { get; private set; }

    public event Action OnCrouchActivated;
    public event Action OnCrouchDeactivated;
    public event Action OnJumpPerformed;
    public event Action OnSprintActivated;
    public event Action OnSprintDeactivated;
    public event Action OnInteractPerformed;
    public event Action OnThrowStarted;
    public event Action OnThrowPerformed;
    public event Action OnDropPerformed;
    public event Action OnWalkToggled;

    public event Action OnBuildTogglePerformed;
    public event Action OnBuildSlot1Performed;
    public event Action OnBuildSlot2Performed;
    public event Action OnBuildSlot3Performed;

    private Controls _controls;

    private void OnEnable()
    {
        if (_controls == null)
        {
            _controls = new Controls();
            _controls.Player.SetCallbacks(this);
        }
        _controls.Player.Enable();
    }

    public void OnDisable()
    {
        _controls.Player.Disable();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        MouseDelta = context.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveComposite = context.ReadValue<Vector2>();
        MovementInputDetected = MoveComposite.magnitude > 0;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnJumpPerformed?.Invoke();
    }

    public void OnToggleWalk(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnWalkToggled?.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnSprintActivated?.Invoke();
        }
        else if (context.canceled)
        {
            OnSprintDeactivated?.Invoke();
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            OnCrouchActivated?.Invoke();
        }
        else if (context.canceled)
        {
            OnCrouchDeactivated?.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnInteractPerformed?.Invoke();
    }

    public void OnThrow(InputAction.CallbackContext context)
    {
        if (context.started)
            OnThrowStarted?.Invoke();

        if (context.canceled)
            OnThrowPerformed?.Invoke();
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        OnDropPerformed?.Invoke();
    }

    public void OnBuildToggle(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnBuildTogglePerformed?.Invoke();
    }

    public void OnBuildSlot1(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnBuildSlot1Performed?.Invoke();
    }

    public void OnBuildSlot2(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnBuildSlot2Performed?.Invoke();
    }

    public void OnBuildSlot3(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        OnBuildSlot3Performed?.Invoke();
    }
}