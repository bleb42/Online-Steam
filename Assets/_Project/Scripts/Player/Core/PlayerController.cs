using UnityEngine;

[RequireComponent(typeof(CharacterController), typeof(PlayerStanceController), typeof(PlayerGroundSensor))]
[RequireComponent(typeof(PlayerInputProcessor), typeof(PlayerMotor), typeof(PlayerOrientation))]
[RequireComponent(typeof(PlayerLookAdditives), typeof(PlayerAnimatorAdapter))]
public class PlayerController : MonoBehaviour
{
    private enum AnimationState
    {
        Base,
        Locomotion,
        Jump,
        Fall,
        Crouch
    }

    [Header("External Components")]
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private Animator _animator;
    [SerializeField] private CharacterController _controller;

    private PlayerStanceController _stance;
    private PlayerGroundSensor _groundSensor;
    private PlayerInputProcessor _inputProcessor;
    private PlayerMotor _motor;
    private PlayerOrientation _orientation;
    private PlayerLookAdditives _lookAdditives;
    private PlayerAnimatorAdapter _animatorAdapter;

    private readonly PlayerLocomotionContext _context = new PlayerLocomotionContext();

    private AnimationState _currentState = AnimationState.Base;
    private float _fallStartTime;

    private void Awake()
    {
        _stance = GetComponent<PlayerStanceController>();
        _groundSensor = GetComponent<PlayerGroundSensor>();
        _inputProcessor = GetComponent<PlayerInputProcessor>();
        _motor = GetComponent<PlayerMotor>();
        _orientation = GetComponent<PlayerOrientation>();
        _lookAdditives = GetComponent<PlayerLookAdditives>();
        _animatorAdapter = GetComponent<PlayerAnimatorAdapter>();

        _groundSensor.Initialize(_controller);
        _stance.Initialize(_inputReader, _controller, _context);
        _inputProcessor.Initialize(_inputReader, _cameraController);
        _motor.Initialize(_controller);
        _orientation.Initialize(_cameraController);
        _lookAdditives.Initialize(_cameraController, _motor.SprintSpeed);
        _animatorAdapter.Initialize(_animator);
    }

    private void Start()
    {
        _stance.Subscribe();

        SwitchState(AnimationState.Locomotion);
    }

    private void OnDestroy()
    {
        _stance.Unsubscribe();
    }

    private void EnterBaseState()
    {
    }

    private void SwitchState(AnimationState newState)
    {
        ExitCurrentState();
        EnterState(newState);
    }

    private void EnterState(AnimationState stateToEnter)
    {
        _currentState = stateToEnter;
        switch (_currentState)
        {
            case AnimationState.Base:
                EnterBaseState();
                break;
            case AnimationState.Locomotion:
                EnterLocomotionState();
                break;
            case AnimationState.Jump:
                EnterJumpState();
                break;
            case AnimationState.Fall:
                EnterFallState();
                break;
            case AnimationState.Crouch:
                EnterCrouchState();
                break;
        }
    }

    private void ExitCurrentState()
    {
        switch (_currentState)
        {
            case AnimationState.Locomotion:
                ExitLocomotionState();
                break;
            case AnimationState.Jump:
                ExitJumpState();
                break;
            case AnimationState.Crouch:
                ExitCrouchState();
                break;
        }
    }

    private void Update()
    {
        switch (_currentState)
        {
            case AnimationState.Locomotion:
                UpdateLocomotionState();
                break;
            case AnimationState.Jump:
                UpdateJumpState();
                break;
            case AnimationState.Fall:
                UpdateFallState();
                break;
            case AnimationState.Crouch:
                UpdateCrouchState();
                break;
        }
    }

    private void EnterLocomotionState()
    {
        _inputReader.OnJumpPerformed += LocomotionToJumpState;
    }

    private void UpdateLocomotionState()
    {
        _groundSensor.UpdateGrounded(_context);

        if (!_context.IsGrounded)
        {
            SwitchState(AnimationState.Fall);
        }

        if (_context.IsCrouching)
        {
            SwitchState(AnimationState.Crouch);
        }

        _lookAdditives.CheckEnableTurns(_context);
        _lookAdditives.CheckEnableLean(_context);
        _lookAdditives.CalculateRotationalAdditives(_context, _lookAdditives.EnableLean, _lookAdditives.EnableHeadTurn, _lookAdditives.EnableBodyTurn);

        _inputProcessor.UpdateInput(_context);
        _motor.UpdateMoveDirection(_context);
        _orientation.CheckIfStarting(_context);
        _orientation.CheckIfStopped(_context);
        _orientation.FaceMoveDirection(_context);
        _motor.Move(_context);
        _animatorAdapter.PushToAnimator(_context);
    }

    private void ExitLocomotionState()
    {
        _inputReader.OnJumpPerformed -= LocomotionToJumpState;
    }

    private void LocomotionToJumpState()
    {
        SwitchState(AnimationState.Jump);
    }

    private void EnterJumpState()
    {
        _animatorAdapter.SetJumping(true);

        _stance.DeactivateSliding();

        _motor.ApplyJumpForce(_context);
    }

    private void UpdateJumpState()
    {
        _motor.ApplyGravity(_context);

        if (_context.Velocity.y <= 0f)
        {
            _animatorAdapter.SetJumping(false);
            SwitchState(AnimationState.Fall);
        }

        _groundSensor.UpdateGrounded(_context);

        _lookAdditives.CalculateRotationalAdditives(_context, false, _lookAdditives.EnableHeadTurn, _lookAdditives.EnableBodyTurn);
        _inputProcessor.UpdateInput(_context);
        _motor.UpdateMoveDirection(_context);
        _orientation.FaceMoveDirection(_context);
        _motor.Move(_context);
        _animatorAdapter.PushToAnimator(_context);
    }

    private void ExitJumpState()
    {
        _animatorAdapter.SetJumping(false);
    }

    private void EnterFallState()
    {
        _fallStartTime = Time.time;
        _context.FallingDuration = 0f;

        Vector3 velocity = _context.Velocity;
        velocity.y = 0f;
        _context.Velocity = velocity;

        _stance.DeactivateCrouch();
        _stance.DeactivateSliding();
    }

    private void UpdateFallState()
    {
        _groundSensor.UpdateGrounded(_context);

        _lookAdditives.CalculateRotationalAdditives(_context, false, _lookAdditives.EnableHeadTurn, _lookAdditives.EnableBodyTurn);

        _inputProcessor.UpdateInput(_context);
        _motor.UpdateMoveDirection(_context);
        _orientation.FaceMoveDirection(_context);

        _motor.ApplyGravity(_context);
        _motor.Move(_context);
        _animatorAdapter.PushToAnimator(_context);

        if (_controller.isGrounded)
        {
            SwitchState(AnimationState.Locomotion);
        }

        _context.FallingDuration = Time.time - _fallStartTime;
    }

    private void EnterCrouchState()
    {
        _inputReader.OnJumpPerformed += CrouchToJumpState;
    }

    private void UpdateCrouchState()
    {
        _groundSensor.UpdateGrounded(_context);
        if (!_context.IsGrounded)
        {
            _stance.DeactivateCrouch();
            _stance.CapsuleCrouchingSize(false);
            SwitchState(AnimationState.Fall);
        }

        _groundSensor.UpdateCeilingCheck(_context);

        if (!_context.CrouchKeyPressed && !_context.CannotStandUp)
        {
            _stance.DeactivateCrouch();
            SwitchToLocomotionState();
        }

        if (!_context.IsCrouching)
        {
            _stance.CapsuleCrouchingSize(false);
            SwitchToLocomotionState();
        }

        _lookAdditives.CheckEnableTurns(_context);
        _lookAdditives.CheckEnableLean(_context);
        _lookAdditives.CalculateRotationalAdditives(_context, false, _lookAdditives.EnableHeadTurn, false);

        _inputProcessor.UpdateInput(_context);
        _motor.UpdateMoveDirection(_context);
        _orientation.CheckIfStarting(_context);
        _orientation.CheckIfStopped(_context);

        _orientation.FaceMoveDirection(_context);
        _motor.Move(_context);
        _animatorAdapter.PushToAnimator(_context);
    }

    private void ExitCrouchState()
    {
        _inputReader.OnJumpPerformed -= CrouchToJumpState;
    }

    private void CrouchToJumpState()
    {
        if (!_context.CannotStandUp)
        {
            _stance.DeactivateCrouch();
            SwitchState(AnimationState.Jump);
        }
    }

    private void SwitchToLocomotionState()
    {
        _stance.DeactivateCrouch();
        SwitchState(AnimationState.Locomotion);
    }

    public void ActivateSliding()
    {
        _stance.ActivateSliding();
    }

    public void DeactivateSliding()
    {
        _stance.DeactivateSliding();
    }
}