using UnityEngine;

public class PlayerAnimatorAdapter : MonoBehaviour
{
    private static readonly int _movementInputTappedHash = Animator.StringToHash("MovementInputTapped");
    private static readonly int _movementInputPressedHash = Animator.StringToHash("MovementInputPressed");
    private static readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");
    private static readonly int _shuffleDirectionXHash = Animator.StringToHash("ShuffleDirectionX");
    private static readonly int _shuffleDirectionZHash = Animator.StringToHash("ShuffleDirectionZ");
    private static readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
    private static readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
    private static readonly int _fallingDurationHash = Animator.StringToHash("FallingDuration");
    private static readonly int _inclineAngleHash = Animator.StringToHash("InclineAngle");
    private static readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
    private static readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
    private static readonly int _forwardStrafeHash = Animator.StringToHash("ForwardStrafe");
    private static readonly int _cameraRotationOffsetHash = Animator.StringToHash("CameraRotationOffset");
    private static readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
    private static readonly int _isTurningInPlaceHash = Animator.StringToHash("IsTurningInPlace");
    private static readonly int _isCrouchingHash = Animator.StringToHash("IsCrouching");
    private static readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
    private static readonly int _isStartingHash = Animator.StringToHash("IsStarting");
    private static readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int _leanValueHash = Animator.StringToHash("LeanValue");
    private static readonly int _headLookXHash = Animator.StringToHash("HeadLookX");
    private static readonly int _headLookYHash = Animator.StringToHash("HeadLookY");
    private static readonly int _bodyLookXHash = Animator.StringToHash("BodyLookX");
    private static readonly int _bodyLookYHash = Animator.StringToHash("BodyLookY");
    private static readonly int _locomotionStartDirectionHash = Animator.StringToHash("LocomotionStartDirection");
    private static readonly int _isHoldingHash = Animator.StringToHash("IsHolding");

    private Animator _animator;

    public void Initialize(Animator animator)
    {
        _animator = animator;
    }

    public void SetJumping(bool isJumping)
    {
        _animator.SetBool(_isJumpingAnimHash, isJumping);
    }

    public void PushToAnimator(PlayerLocomotionContext context)
    {
        _animator.SetFloat(_leanValueHash, context.LeanValue);
        _animator.SetFloat(_headLookXHash, context.HeadLookX);
        _animator.SetFloat(_headLookYHash, context.HeadLookY);
        _animator.SetFloat(_bodyLookXHash, context.BodyLookX);
        _animator.SetFloat(_bodyLookYHash, context.BodyLookY);

        _animator.SetFloat(_isStrafingHash, context.IsStrafing ? 1.0f : 0.0f);

        _animator.SetFloat(_inclineAngleHash, context.InclineAngle);

        _animator.SetFloat(_moveSpeedHash, context.Speed2D);
        _animator.SetInteger(_currentGaitHash, (int)context.CurrentGait);

        _animator.SetFloat(_strafeDirectionXHash, context.StrafeDirectionX);
        _animator.SetFloat(_strafeDirectionZHash, context.StrafeDirectionZ);
        _animator.SetFloat(_forwardStrafeHash, context.ForwardStrafe);
        _animator.SetFloat(_cameraRotationOffsetHash, context.CameraRotationOffset);

        _animator.SetBool(_movementInputHeldHash, context.MovementInputHeld);
        _animator.SetBool(_movementInputPressedHash, context.MovementInputPressed);
        _animator.SetBool(_movementInputTappedHash, context.MovementInputTapped);
        _animator.SetFloat(_shuffleDirectionXHash, context.ShuffleDirectionX);
        _animator.SetFloat(_shuffleDirectionZHash, context.ShuffleDirectionZ);

        _animator.SetBool(_isTurningInPlaceHash, context.IsTurningInPlace);
        _animator.SetBool(_isCrouchingHash, context.IsCrouching);

        _animator.SetFloat(_fallingDurationHash, context.FallingDuration);
        _animator.SetBool(_isGroundedHash, context.IsGrounded);

        _animator.SetBool(_isWalkingHash, context.IsWalking);
        _animator.SetBool(_isStoppedHash, context.IsStopped);
        _animator.SetBool(_isStartingHash, context.IsStarting);

        _animator.SetFloat(_locomotionStartDirectionHash, context.LocomotionStartDirection);
    }

    public void SetHolding(bool isHolding)
    {
        _animator.SetBool(_isHoldingHash, isHolding);
    }
}