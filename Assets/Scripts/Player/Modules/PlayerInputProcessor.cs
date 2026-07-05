using UnityEngine;

public class PlayerInputProcessor : MonoBehaviour
{
    [Header("Shuffles")]
    [SerializeField] private float _buttonHoldThreshold = 0.15f;

    private InputReader _inputReader;
    private CameraController _cameraController;

    public void Initialize(InputReader inputReader, CameraController cameraController)
    {
        _inputReader = inputReader;
        _cameraController = cameraController;
    }

    public void UpdateInput(PlayerLocomotionContext context)
    {
        if (_inputReader.MovementInputDetected)
        {
            if (_inputReader.MovementInputDuration == 0)
            {
                context.MovementInputTapped = true;
            }
            else if (_inputReader.MovementInputDuration > 0 && _inputReader.MovementInputDuration < _buttonHoldThreshold)
            {
                context.MovementInputTapped = false;
                context.MovementInputPressed = true;
                context.MovementInputHeld = false;
            }
            else
            {
                context.MovementInputTapped = false;
                context.MovementInputPressed = false;
                context.MovementInputHeld = true;
            }

            _inputReader.MovementInputDuration += Time.deltaTime;
        }
        else
        {
            _inputReader.MovementInputDuration = 0;
            context.MovementInputTapped = false;
            context.MovementInputPressed = false;
            context.MovementInputHeld = false;
        }

        context.MoveDirection = (_cameraController.GetCameraForwardZeroedYNormalised() * _inputReader.MoveComposite.y)
            + (_cameraController.GetCameraRightZeroedYNormalised() * _inputReader.MoveComposite.x);
    }
}