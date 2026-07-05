using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    [Header("Player Locomotion")]
    [SerializeField] private float _walkSpeed = 1.4f;
    [SerializeField] private float _runSpeed = 2.5f;
    [SerializeField] private float _sprintSpeed = 7f;
    [SerializeField] private float _speedChangeDamping = 10f;

    [Header("Player In-Air")]
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _gravityMultiplier = 2f;

    private const float _ANIMATION_DAMP_TIME = 5f;

    private CharacterController _controller;
    private Vector3 _targetVelocity;

    public float SprintSpeed => _sprintSpeed;

    public void Initialize(CharacterController controller)
    {
        _controller = controller;
    }

    public void UpdateMoveDirection(PlayerLocomotionContext context)
    {
        float targetMaxSpeed;

        if (!context.IsGrounded)
        {
            targetMaxSpeed = context.CurrentMaxSpeed;
        }
        else if (context.IsCrouching)
        {
            targetMaxSpeed = _walkSpeed;
        }
        else if (context.IsSprinting)
        {
            targetMaxSpeed = _sprintSpeed;
        }
        else if (context.IsWalking)
        {
            targetMaxSpeed = _walkSpeed;
        }
        else
        {
            targetMaxSpeed = _runSpeed;
        }

        context.CurrentMaxSpeed = Mathf.Lerp(context.CurrentMaxSpeed, targetMaxSpeed, _ANIMATION_DAMP_TIME * Time.deltaTime);

        _targetVelocity.x = context.MoveDirection.x * context.CurrentMaxSpeed;
        _targetVelocity.z = context.MoveDirection.z * context.CurrentMaxSpeed;

        context.Velocity = new Vector3(
            Mathf.Lerp(context.Velocity.x, _targetVelocity.x, _speedChangeDamping * Time.deltaTime),
            context.Velocity.y,
            Mathf.Lerp(context.Velocity.z, _targetVelocity.z, _speedChangeDamping * Time.deltaTime)
        );

        context.Speed2D = new Vector3(context.Velocity.x, 0f, context.Velocity.z).magnitude;
        context.Speed2D = Mathf.Round(context.Speed2D * 1000f) / 1000f;

        Vector3 playerForwardVector = transform.forward;
        context.NewDirectionDifferenceAngle = playerForwardVector != context.MoveDirection
            ? Vector3.SignedAngle(playerForwardVector, context.MoveDirection, Vector3.up)
            : 0f;

        UpdateGait(context);
    }

    private void UpdateGait(PlayerLocomotionContext context)
    {
        float runThreshold = (_walkSpeed + _runSpeed) / 2;
        float sprintThreshold = (_runSpeed + _sprintSpeed) / 2;

        if (context.Speed2D < 0.01f)
        {
            context.CurrentGait = SampleGaitState.Idle;
        }
        else if (context.Speed2D < runThreshold)
        {
            context.CurrentGait = SampleGaitState.Walk;
        }
        else if (context.Speed2D < sprintThreshold)
        {
            context.CurrentGait = SampleGaitState.Run;
        }
        else
        {
            context.CurrentGait = SampleGaitState.Sprint;
        }
    }

    public void ApplyGravity(PlayerLocomotionContext context)
    {
        Vector3 velocity = context.Velocity;

        if (velocity.y > Physics.gravity.y)
        {
            velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }

        context.Velocity = velocity;
    }

    public void Move(PlayerLocomotionContext context)
    {
        _controller.Move(context.Velocity * Time.deltaTime);
    }

    public void ApplyJumpForce(PlayerLocomotionContext context)
    {
        context.Velocity = new Vector3(context.Velocity.x, _jumpForce, context.Velocity.z);
    }
}