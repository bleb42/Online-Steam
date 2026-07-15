using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const int _LAG_DELTA_TIME_ADJUSTMENT = 20;

    [Header("External Components")]
    [SerializeField] private GameObject _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _playerTarget;
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerHand _playerHand;

    [Header("Camera Settings")]
    [SerializeField] private bool _hideCursor;
    [SerializeField] private float _cameraDistance = 2f;
    [SerializeField] private float _cameraHeightOffset;
    [SerializeField] private float _cameraHorizontalOffset;
    [SerializeField] private float _cameraTiltOffset;
    [SerializeField] private Vector2 _cameraTiltBounds = new Vector2(-10f, 45f);
    [SerializeField] private float _positionalCameraLag = 1f;
    [SerializeField] private float _rotationalCameraLag = 1f;

    [Header("Camera Collision")]
    [SerializeField] private LayerMask _cameraCollisionMask;
    [SerializeField] private float _cameraCollisionRadius = 0.2f;
    [SerializeField] private float _cameraCollisionBuffer = 0.15f;
    [SerializeField] private float _minCameraDistance = 0.2f;

    [Header("Aim Zoom")]
    [SerializeField] private float _aimCameraDistance = 3.5f;
    [SerializeField] private float _aimHeightOffset = 0.3f;
    [SerializeField] private float _aimHorizontalOffset = 0.5f;
    [SerializeField] private float _aimFovOffset = 0f;
    [SerializeField] private float _aimTransitionSpeed = 8f;

    private Vector3 _lastPosition;
    private Vector3 _newPosition;
    private float _cameraInversion;
    private float _mouseSensitivity;
    private float _lastAngleX;
    private float _lastAngleY;
    private float _newAngleX;
    private float _newAngleY;
    private float _rotationX;
    private float _rotationY;
    private Transform _camera;

    private float _currentCameraDistance;
    private float _currentHeightOffset;
    private float _currentHorizontalOffset;
    private float _baseFov;

    private void Start()
    {
        ApplySettings();

        if (GameSettingsService.Instance != null)
            GameSettingsService.Instance.OnSettingsChanged += ApplySettings;

        _camera = gameObject.transform.GetChild(0);

        if (_hideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        transform.position = _playerTarget.position;
        transform.rotation = _playerTarget.rotation;

        _lastPosition = transform.position;

        _currentCameraDistance = _cameraDistance;
        _currentHeightOffset = _cameraHeightOffset;
        _currentHorizontalOffset = _cameraHorizontalOffset;
        _baseFov = _mainCamera.fieldOfView;

        UpdateCameraCollision();
    }

    private void Update()
    {
        if (_inputReader == null) { Debug.LogError("[CameraController] _inputReader is NULL"); return; }

        float positionalFollowSpeed = 1 / (_positionalCameraLag / _LAG_DELTA_TIME_ADJUSTMENT);
        float rotationalFollowSpeed = 1 / (_rotationalCameraLag / _LAG_DELTA_TIME_ADJUSTMENT);

        _rotationX = _inputReader.MouseDelta.y * _cameraInversion * _mouseSensitivity;
        _rotationY = _inputReader.MouseDelta.x * _mouseSensitivity;

        _newAngleX += _rotationX;
        _newAngleX = Mathf.Clamp(_newAngleX, _cameraTiltBounds.x, _cameraTiltBounds.y);
        _newAngleX = Mathf.Lerp(_lastAngleX, _newAngleX, rotationalFollowSpeed * Time.deltaTime);

        _newAngleY += _rotationY;
        _newAngleY = Mathf.Lerp(_lastAngleY, _newAngleY, rotationalFollowSpeed * Time.deltaTime);

        _newPosition = _playerTarget.position;
        _newPosition = Vector3.Lerp(_lastPosition, _newPosition, positionalFollowSpeed * Time.deltaTime);

        transform.position = _newPosition;
        transform.eulerAngles = new Vector3(_newAngleX, _newAngleY, 0);

        UpdateAimZoom();
        UpdateCameraCollision();

        _lastPosition = _newPosition;
        _lastAngleX = _newAngleX;
        _lastAngleY = _newAngleY;
    }

    private void OnDestroy()
    {
        if (GameSettingsService.Instance != null)
            GameSettingsService.Instance.OnSettingsChanged -= ApplySettings;
    }

    public Vector3 GetCameraForwardZeroedY()
    {
        return new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z);
    }

    public Vector3 GetCameraForwardZeroedYNormalised()
    {
        return GetCameraForwardZeroedY().normalized;
    }

    public Vector3 GetCameraRightZeroedY()
    {
        return new Vector3(_mainCamera.transform.right.x, 0, _mainCamera.transform.right.z);
    }

    public Vector3 GetCameraRightZeroedYNormalised()
    {
        return GetCameraRightZeroedY().normalized;
    }

    public float GetCameraTiltX()
    {
        return _mainCamera.transform.eulerAngles.x;
    }

    private void ApplySettings()
    {
        var settings = GameSettingsService.Instance.Current;
        _mouseSensitivity = settings.MouseSensitivity;
        _cameraInversion = settings.InvertCamera ? 1 : -1;
    }

    private void UpdateCameraCollision()
    {
        Vector3 desiredLocalPosition = new Vector3(_currentHorizontalOffset, _currentHeightOffset, -_currentCameraDistance);
        Vector3 desiredWorldPosition = transform.TransformPoint(desiredLocalPosition);

        Vector3 direction = desiredWorldPosition - transform.position;
        float desiredDistance = direction.magnitude;
        Vector3 directionNormalised = direction.normalized;

        float actualDistance = desiredDistance;

        if (Physics.SphereCast(
                transform.position,
                _cameraCollisionRadius,
                directionNormalised,
                out RaycastHit hit,
                desiredDistance,
                _cameraCollisionMask,
                QueryTriggerInteraction.Ignore
            ))
        {
            actualDistance = Mathf.Clamp(hit.distance - _cameraCollisionBuffer, _minCameraDistance, desiredDistance);
        }

        _camera.position = transform.position + directionNormalised * actualDistance;
        _camera.localEulerAngles = new Vector3(_cameraTiltOffset, 0f, 0f);
    }
    private void UpdateAimZoom()
    {
        bool isAiming = _playerHand != null && !_playerHand.IsEmpty && _playerHand.IsAiming;

        float targetDistance = isAiming ? _aimCameraDistance : _cameraDistance;
        float targetHeight = isAiming ? _cameraHeightOffset + _aimHeightOffset : _cameraHeightOffset;
        float targetHorizontal = isAiming ? _cameraHorizontalOffset + _aimHorizontalOffset : _cameraHorizontalOffset;

        _currentCameraDistance = Mathf.Lerp(_currentCameraDistance, targetDistance, _aimTransitionSpeed * Time.deltaTime);
        _currentHeightOffset = Mathf.Lerp(_currentHeightOffset, targetHeight, _aimTransitionSpeed * Time.deltaTime);
        _currentHorizontalOffset = Mathf.Lerp(_currentHorizontalOffset, targetHorizontal, _aimTransitionSpeed * Time.deltaTime);

        if (_aimFovOffset != 0f)
        {
            float targetFov = isAiming ? _baseFov + _aimFovOffset : _baseFov;
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFov, _aimTransitionSpeed * Time.deltaTime);
        }
    }
}