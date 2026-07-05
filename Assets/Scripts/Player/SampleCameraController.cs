using UnityEngine;

public class SampleCameraController : MonoBehaviour
{
    private const int _LAG_DELTA_TIME_ADJUSTMENT = 20;

    [Header("External Components")]
    [SerializeField] private GameObject _character;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Transform _playerTarget;

    [Header("Camera Settings")]
    [SerializeField] private bool _invertCamera;
    [SerializeField] private bool _hideCursor;
    [SerializeField] private float _mouseSensitivity = 5f;
    [SerializeField] private float _cameraDistance = 5f;
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

    private InputReader _inputReader;
    private Vector3 _lastPosition;
    private Vector3 _newPosition;
    private float _cameraInversion;
    private float _lastAngleX;
    private float _lastAngleY;
    private float _newAngleX;
    private float _newAngleY;
    private float _rotationX;
    private float _rotationY;
    private Transform _camera;

    private void Start()
    {
        _camera = gameObject.transform.GetChild(0);

        _inputReader = _character.GetComponent<InputReader>();

        if (_hideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        _cameraInversion = _invertCamera ? 1 : -1;

        transform.position = _playerTarget.position;
        transform.rotation = _playerTarget.rotation;

        _lastPosition = transform.position;

        UpdateCameraCollision();
    }

    private void Update()
    {
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

        UpdateCameraCollision();

        _lastPosition = _newPosition;
        _lastAngleX = _newAngleX;
        _lastAngleY = _newAngleY;
    }

    private void UpdateCameraCollision()
    {
        Vector3 desiredLocalPosition = new Vector3(_cameraHorizontalOffset, _cameraHeightOffset, -_cameraDistance);
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
}