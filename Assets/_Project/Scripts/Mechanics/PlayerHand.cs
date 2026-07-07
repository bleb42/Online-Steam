using UnityEngine;

public class PlayerHand : MonoBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private TrajectoryPredictor _trajectoryPredictor;
    [SerializeField] private PlayerAnimatorAdapter _animatorAdapter;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _throwAngleUp = 15f;
    [SerializeField] private float _throwForce = 10f;

    public ITakeable HeldItem { get; private set; }
    public bool IsEmpty => HeldItem == null;
    public float ThrowForce => _throwForce;

    private bool _isAiming;

    private void OnEnable()
    {
        _inputReader.OnThrowStarted += StartAim;
        _inputReader.OnThrowPerformed += Throw;
        _inputReader.OnDropPerformed += Drop;
    }

    private void OnDisable()
    {
        _inputReader.OnThrowStarted -= StartAim;
        _inputReader.OnThrowPerformed -= Throw;
        _inputReader.OnDropPerformed -= Drop;
    }

    private void Update()
    {
        if (!_isAiming || HeldItem == null) 
            return;

        var item = HeldItem as MonoBehaviour;

        if (item == null) 
            return;

        Vector3 direction = GetThrowDirection();
        _trajectoryPredictor.ShowTrajectory(item.transform.position, direction * _throwForce);
    }

    public void Hold(ITakeable item)
    {
        HeldItem = item;
        _animatorAdapter.SetHolding(true);
    }

    public void Clear()
    {
        HeldItem = null;
        _animatorAdapter.SetHolding(false);
    }

    private void StartAim()
    {
        if (HeldItem == null) 
            return;

        _isAiming = true;
    }

    private void Throw()
    {
        _isAiming = false;

        if (HeldItem == null) 
            return;

        _trajectoryPredictor.HideTrajectory();

        var rb = (HeldItem as MonoBehaviour)?.GetComponent<Rigidbody>();
        Vector3 direction = GetThrowDirection();

        HeldItem.Use();

        if (rb != null)
            rb.linearVelocity = direction * _throwForce; 

        Clear();
    }

    private void Drop()
    {
        _isAiming = false;

        if (HeldItem == null) 
            return;

        _trajectoryPredictor.HideTrajectory();
        HeldItem.Drop();
        
        Clear();    
    }

    private Vector3 GetThrowDirection()
    {
        Vector3 dir = _camera.transform.forward;
        dir = Quaternion.AngleAxis(-_throwAngleUp, _camera.transform.right) * dir;
        return dir;
    }
}