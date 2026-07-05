using UnityEngine;

public class PlayerStanceController : MonoBehaviour
{
    [Header("Player Locomotion")]
    [SerializeField] private bool _alwaysStrafe = true;

    [Header("Capsule Values")]
    [SerializeField] private float _capsuleStandingHeight = 1.8f;
    [SerializeField] private float _capsuleStandingCentre = 0.93f;
    [SerializeField] private float _capsuleCrouchingHeight = 1.2f;
    [SerializeField] private float _capsuleCrouchingCentre = 0.6f;

    private InputReader _inputReader;
    private CharacterController _controller;
    private PlayerLocomotionContext _context;
    private bool _subscribed;

    public void Initialize(InputReader inputReader, CharacterController controller, PlayerLocomotionContext context)
    {
        _inputReader = inputReader;
        _controller = controller;
        _context = context;

        _context.IsStrafing = _alwaysStrafe;
    }

    public void Subscribe()
    {
        if (_subscribed)
        {
            return;
        }

        _inputReader.OnWalkToggled += ToggleWalk;
        _inputReader.OnSprintActivated += ActivateSprint;
        _inputReader.OnSprintDeactivated += DeactivateSprint;
        _inputReader.OnCrouchActivated += ActivateCrouch;
        _inputReader.OnCrouchDeactivated += DeactivateCrouch;
        _inputReader.OnAimActivated += ActivateAim;
        _inputReader.OnAimDeactivated += DeactivateAim;

        _subscribed = true;
    }

    public void Unsubscribe()
    {
        if (!_subscribed)
        {
            return;
        }

        _inputReader.OnWalkToggled -= ToggleWalk;
        _inputReader.OnSprintActivated -= ActivateSprint;
        _inputReader.OnSprintDeactivated -= DeactivateSprint;
        _inputReader.OnCrouchActivated -= ActivateCrouch;
        _inputReader.OnCrouchDeactivated -= DeactivateCrouch;
        _inputReader.OnAimActivated -= ActivateAim;
        _inputReader.OnAimDeactivated -= DeactivateAim;

        _subscribed = false;
    }

    private void ActivateAim()
    {
        _context.IsAiming = true;
        _context.IsStrafing = !_context.IsSprinting;
    }

    private void DeactivateAim()
    {
        _context.IsAiming = false;
        _context.IsStrafing = !_context.IsSprinting && _alwaysStrafe;
    }

    private void ToggleWalk()
    {
        EnableWalk(!_context.IsWalking);
    }

    private void EnableWalk(bool enable)
    {
        _context.IsWalking = enable && _context.IsGrounded && !_context.IsSprinting;
    }

    private void ActivateSprint()
    {
        if (!_context.IsCrouching)
        {
            EnableWalk(false);
            _context.IsSprinting = true;
            _context.IsStrafing = false;
        }
    }

    private void DeactivateSprint()
    {
        _context.IsSprinting = false;

        if (_alwaysStrafe || _context.IsAiming)
        {
            _context.IsStrafing = true;
        }
    }

    private void ActivateCrouch()
    {
        _context.CrouchKeyPressed = true;

        if (_context.IsGrounded)
        {
            CapsuleCrouchingSize(true);
            DeactivateSprint();
            _context.IsCrouching = true;
        }
    }

    public void DeactivateCrouch()
    {
        _context.CrouchKeyPressed = false;

        if (!_context.CannotStandUp && !_context.IsSliding)
        {
            CapsuleCrouchingSize(false);
            _context.IsCrouching = false;
        }
    }

    public void ActivateSliding()
    {
        _context.IsSliding = true;
    }

    public void DeactivateSliding()
    {
        _context.IsSliding = false;
    }

    public void CapsuleCrouchingSize(bool crouching)
    {
        if (crouching)
        {
            _controller.center = new Vector3(0f, _capsuleCrouchingCentre, 0f);
            _controller.height = _capsuleCrouchingHeight;
        }
        else
        {
            _controller.center = new Vector3(0f, _capsuleStandingCentre, 0f);
            _controller.height = _capsuleStandingHeight;
        }
    }
}