using UnityEngine;

public class PlayerLookAdditives : MonoBehaviour
{
    [Header("Player Head Look")]
    [SerializeField] private AnimationCurve _headLookXCurve;

    [Header("Player Body Look")]
    [SerializeField] private AnimationCurve _bodyLookXCurve;

    [Header("Player Lean")]
    [SerializeField] private AnimationCurve _leanCurve;

    private CameraController _cameraController;
    private float _sprintSpeedReference;

    private bool _enableHeadTurn = true;
    private bool _enableBodyTurn = true;
    private bool _enableLean = true;

    private Vector3 _previousRotation;
    private Vector3 _currentRotation;
    private float _rotationRate;
    private float _initialLeanValue;
    private float _initialTurnValue;

    public bool EnableHeadTurn => _enableHeadTurn;
    public bool EnableBodyTurn => _enableBodyTurn;
    public bool EnableLean => _enableLean;

    public void Initialize(CameraController cameraController, float sprintSpeed)
    {
        _cameraController = cameraController;
        _sprintSpeedReference = sprintSpeed;
        _previousRotation = transform.forward;
    }

    public void CheckEnableTurns(PlayerLocomotionContext context)
    {
        context.HeadLookDelay = LocomotionMath.VariableOverrideDelayTimer(context.HeadLookDelay);
        _enableHeadTurn = context.HeadLookDelay == 0.0f && !context.IsStarting;

        context.BodyLookDelay = LocomotionMath.VariableOverrideDelayTimer(context.BodyLookDelay);
        _enableBodyTurn = context.BodyLookDelay == 0.0f && !(context.IsStarting || context.IsTurningInPlace);
    }

    public void CheckEnableLean(PlayerLocomotionContext context)
    {
        context.LeanDelay = LocomotionMath.VariableOverrideDelayTimer(context.LeanDelay);
        _enableLean = context.LeanDelay == 0.0f && !(context.IsStarting || context.IsTurningInPlace);
    }

    public void CalculateRotationalAdditives(PlayerLocomotionContext context, bool leansActivated, bool headLookActivated, bool bodyLookActivated)
    {
        if (headLookActivated || leansActivated || bodyLookActivated)
        {
            _currentRotation = transform.forward;

            _rotationRate = _currentRotation != _previousRotation
                ? Vector3.SignedAngle(_currentRotation, _previousRotation, Vector3.up) / Time.deltaTime * -1f
                : 0f;
        }

        _initialLeanValue = leansActivated ? _rotationRate : 0f;

        float leanSmoothness = 5;
        float maxLeanRotationRate = 275.0f;
        float referenceValue = context.Speed2D / _sprintSpeedReference;

        context.LeanValue = CalculateSmoothedValue(context.LeanValue, _initialLeanValue, maxLeanRotationRate, leanSmoothness, _leanCurve, referenceValue, true);

        float headTurnSmoothness = 5f;

        if (headLookActivated && context.IsTurningInPlace)
        {
            _initialTurnValue = context.CameraRotationOffset;
            context.HeadLookX = Mathf.Lerp(context.HeadLookX, _initialTurnValue / 200, 5f * Time.deltaTime);
        }
        else
        {
            _initialTurnValue = headLookActivated ? _rotationRate : 0f;
            context.HeadLookX = CalculateSmoothedValue(context.HeadLookX, _initialTurnValue, maxLeanRotationRate, headTurnSmoothness, _headLookXCurve, context.HeadLookX, false);
        }

        float bodyTurnSmoothness = 5f;
        _initialTurnValue = bodyLookActivated ? _rotationRate : 0f;
        context.BodyLookX = CalculateSmoothedValue(context.BodyLookX, _initialTurnValue, maxLeanRotationRate, bodyTurnSmoothness, _bodyLookXCurve, context.BodyLookX, false);

        float cameraTilt = _cameraController.GetCameraTiltX();
        cameraTilt = (cameraTilt > 180f ? cameraTilt - 360f : cameraTilt) / -180;
        cameraTilt = Mathf.Clamp(cameraTilt, -0.1f, 1.0f);
        context.HeadLookY = cameraTilt;
        context.BodyLookY = cameraTilt;

        _previousRotation = _currentRotation;
    }

    private float CalculateSmoothedValue(
        float mainVariable,
        float newValue,
        float maxRateChange,
        float smoothness,
        AnimationCurve referenceCurve,
        float referenceValue,
        bool isMultiplier
    )
    {
        float changeVariable = newValue / maxRateChange;
        changeVariable = Mathf.Clamp(changeVariable, -1.0f, 1.0f);

        if (isMultiplier)
        {
            float multiplier = referenceCurve.Evaluate(referenceValue);
            changeVariable *= multiplier;
        }
        else
        {
            changeVariable = referenceCurve.Evaluate(changeVariable);
        }

        if (!changeVariable.Equals(mainVariable))
        {
            changeVariable = Mathf.Lerp(mainVariable, changeVariable, smoothness * Time.deltaTime);
        }

        return changeVariable;
    }
}