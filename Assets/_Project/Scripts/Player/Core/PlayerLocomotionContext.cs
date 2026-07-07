using UnityEngine;

public class PlayerLocomotionContext
{
    public Vector3 MoveDirection;
    public bool MovementInputTapped;
    public bool MovementInputPressed;
    public bool MovementInputHeld;

    public bool IsWalking;
    public bool IsSprinting;
    public bool IsCrouching;
    public bool IsAiming;
    public bool IsStrafing;
    public bool IsSliding;
    public bool CrouchKeyPressed;
    public bool CannotStandUp;

    public Vector3 Velocity;
    public float Speed2D;
    public float CurrentMaxSpeed;
    public float NewDirectionDifferenceAngle;
    public SampleGaitState CurrentGait;

    public bool IsGrounded = true;
    public float InclineAngle;

    public float StrafeAngle;
    public float StrafeDirectionX;
    public float StrafeDirectionZ = 1f;
    public float ForwardStrafe = 1f;
    public float CameraRotationOffset;
    public bool IsTurningInPlace;
    public float ShuffleDirectionX;
    public float ShuffleDirectionZ;

    public bool IsStarting;
    public bool IsStopped = true;
    public float LocomotionStartDirection;

    public float LeanDelay;
    public float HeadLookDelay;
    public float BodyLookDelay;

    public float FallingDuration;

    public float LeanValue;
    public float HeadLookX;
    public float HeadLookY;
    public float BodyLookX;
    public float BodyLookY;
}

public enum SampleGaitState
{
    Idle,
    Walk,
    Run,
    Sprint
}