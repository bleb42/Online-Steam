using UnityEngine;

public class PlayerOrientation : MonoBehaviour
{
    [Header("Player Rotation")]
    [SerializeField] private float _rotationSmoothing = 10f;

    [Header("Player Strafing")]
    [SerializeField] private float _forwardStrafeMinThreshold = -55.0f;
    [SerializeField] private float _forwardStrafeMaxThreshold = 125.0f;

    private const float _ANIMATION_DAMP_TIME = 5f;
    private const float _STRAFE_DIRECTION_DAMP_TIME = 20f;

    private CameraController _cameraController;
    private float _locomotionStartTimer;

    public void Initialize(CameraController cameraController)
    {
        _cameraController = cameraController;
    }

    public void FaceMoveDirection(PlayerLocomotionContext context)
    {
        Vector3 characterForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 characterRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;
        Vector3 directionForward = new Vector3(context.MoveDirection.x, 0f, context.MoveDirection.z).normalized;

        Vector3 cameraForward = _cameraController.GetCameraForwardZeroedYNormalised();
        Quaternion strafingTargetRotation = Quaternion.LookRotation(cameraForward);

        context.StrafeAngle = characterForward != directionForward
            ? Vector3.SignedAngle(characterForward, directionForward, Vector3.up)
            : 0f;

        context.IsTurningInPlace = false;

        if (context.IsStrafing)
        {
            if (context.MoveDirection.magnitude > 0.01f)
            {
                if (cameraForward != Vector3.zero)
                {
                    context.ShuffleDirectionZ = Vector3.Dot(characterForward, directionForward);
                    context.ShuffleDirectionX = Vector3.Dot(characterRight, directionForward);

                    UpdateStrafeDirection(context, Vector3.Dot(characterForward, directionForward), Vector3.Dot(characterRight, directionForward));
                    context.CameraRotationOffset = Mathf.Lerp(context.CameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

                    float targetValue = context.StrafeAngle > _forwardStrafeMinThreshold && context.StrafeAngle < _forwardStrafeMaxThreshold ? 1f : 0f;

                    if (Mathf.Abs(context.ForwardStrafe - targetValue) <= 0.001f)
                    {
                        context.ForwardStrafe = targetValue;
                    }
                    else
                    {
                        float t = Mathf.Clamp01(_STRAFE_DIRECTION_DAMP_TIME * Time.deltaTime);
                        context.ForwardStrafe = Mathf.SmoothStep(context.ForwardStrafe, targetValue, t);
                    }
                }

                transform.rotation = Quaternion.Slerp(transform.rotation, strafingTargetRotation, _rotationSmoothing * Time.deltaTime);
            }
            else
            {
                UpdateStrafeDirection(context, 1f, 0f);

                float t = 20 * Time.deltaTime;
                float newOffset = 0f;

                if (characterForward != cameraForward)
                {
                    newOffset = Vector3.SignedAngle(characterForward, cameraForward, Vector3.up);
                }

                context.CameraRotationOffset = Mathf.Lerp(context.CameraRotationOffset, newOffset, t);

                if (Mathf.Abs(context.CameraRotationOffset) > 10)
                {
                    context.IsTurningInPlace = true;
                }
            }
        }
        else
        {
            UpdateStrafeDirection(context, 1f, 0f);
            context.CameraRotationOffset = Mathf.Lerp(context.CameraRotationOffset, 0f, _rotationSmoothing * Time.deltaTime);

            context.ShuffleDirectionZ = 1;
            context.ShuffleDirectionX = 0;

            Vector3 faceDirection = new Vector3(context.Velocity.x, 0f, context.Velocity.z);

            if (faceDirection == Vector3.zero)
            {
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(faceDirection),
                _rotationSmoothing * Time.deltaTime
            );
        }
    }

    private void UpdateStrafeDirection(PlayerLocomotionContext context, float targetZ, float targetX)
    {
        context.StrafeDirectionZ = Mathf.Lerp(context.StrafeDirectionZ, targetZ, _ANIMATION_DAMP_TIME * Time.deltaTime);
        context.StrafeDirectionX = Mathf.Lerp(context.StrafeDirectionX, targetX, _ANIMATION_DAMP_TIME * Time.deltaTime);
        context.StrafeDirectionZ = Mathf.Round(context.StrafeDirectionZ * 1000f) / 1000f;
        context.StrafeDirectionX = Mathf.Round(context.StrafeDirectionX * 1000f) / 1000f;
    }

    public void CheckIfStopped(PlayerLocomotionContext context)
    {
        context.IsStopped = context.MoveDirection.magnitude == 0 && context.Speed2D < .5f;
    }

    public void CheckIfStarting(PlayerLocomotionContext context)
    {
        _locomotionStartTimer = LocomotionMath.VariableOverrideDelayTimer(_locomotionStartTimer);

        bool isStartingCheck = false;

        if (_locomotionStartTimer <= 0.0f)
        {
            if (context.MoveDirection.magnitude > 0.01f && context.Speed2D < 1 && !context.IsStrafing)
            {
                isStartingCheck = true;
            }

            if (isStartingCheck)
            {
                if (!context.IsStarting)
                {
                    context.LocomotionStartDirection = context.NewDirectionDifferenceAngle;
                }

                float delayTime = 0.2f;
                context.LeanDelay = delayTime;
                context.HeadLookDelay = delayTime;
                context.BodyLookDelay = delayTime;

                _locomotionStartTimer = delayTime;
            }
        }
        else
        {
            isStartingCheck = true;
        }

        context.IsStarting = isStartingCheck;
    }
}