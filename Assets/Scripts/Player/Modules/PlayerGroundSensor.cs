using UnityEngine;

public class PlayerGroundSensor : MonoBehaviour
{
    [Header("Grounded Angle")]
    [SerializeField] private Transform _rearRayPos;
    [SerializeField] private Transform _frontRayPos;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _groundedOffset = -0.14f;

    [Header("Capsule Values (для проверки потолка)")]
    [SerializeField] private float _capsuleStandingHeight = 1.8f;

    private CharacterController _controller;

    public void Initialize(CharacterController controller)
    {
        _controller = controller;
    }

    public void UpdateGrounded(PlayerLocomotionContext context)
    {
        Vector3 spherePosition = new Vector3(
            _controller.transform.position.x,
            _controller.transform.position.y - _groundedOffset,
            _controller.transform.position.z
        );

        context.IsGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);

        if (context.IsGrounded)
        {
            UpdateGroundIncline(context);
        }
    }

    private void UpdateGroundIncline(PlayerLocomotionContext context)
    {
        float rayDistance = Mathf.Infinity;
        _rearRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);
        _frontRayPos.rotation = Quaternion.Euler(transform.rotation.x, 0, 0);

        Physics.Raycast(_rearRayPos.position, _rearRayPos.TransformDirection(-Vector3.up), out RaycastHit rearHit, rayDistance, _groundLayerMask);
        Physics.Raycast(
            _frontRayPos.position,
            _frontRayPos.TransformDirection(-Vector3.up),
            out RaycastHit frontHit,
            rayDistance,
            _groundLayerMask
        );

        Vector3 hitDifference = frontHit.point - rearHit.point;
        float xPlaneLength = new Vector2(hitDifference.x, hitDifference.z).magnitude;

        context.InclineAngle = Mathf.Lerp(context.InclineAngle, Mathf.Atan2(hitDifference.y, xPlaneLength) * Mathf.Rad2Deg, 20f * Time.deltaTime);
    }

    public void UpdateCeilingCheck(PlayerLocomotionContext context)
    {
        float rayDistance = Mathf.Infinity;
        float minimumStandingHeight = _capsuleStandingHeight - _frontRayPos.localPosition.y;

        Vector3 midpoint = new Vector3(transform.position.x, transform.position.y + _frontRayPos.localPosition.y, transform.position.z);
        if (Physics.Raycast(midpoint, transform.TransformDirection(Vector3.up), out RaycastHit ceilingHit, rayDistance, _groundLayerMask))
        {
            context.CannotStandUp = ceilingHit.distance < minimumStandingHeight;
        }
        else
        {
            context.CannotStandUp = false;
        }
    }
}