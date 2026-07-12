using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Outline))]
public abstract class TakeableItem : NetworkBehaviour, ITakeable
{
    [SerializeField] protected ItemData _data;
    [SerializeField] protected Collider _physicalCollider;
    [SerializeField] private NetworkTransform _networkTransform;

    protected Rigidbody _rigidbody;
    protected Outline _outline;

    private readonly SyncVar<bool> _isHeld = new SyncVar<bool>();
    private readonly SyncVar<NetworkObject> _holderObject = new SyncVar<NetworkObject>();

    private NetworkConnection _holderConnection;
    private RigidbodyInterpolation _defaultInterpolation;
    private int _usesLeft;

    public bool IsHeld => _isHeld.Value;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
        _usesLeft = _data.MaxUses;
        _defaultInterpolation = _rigidbody.interpolation;
    }

    private void OnEnable()
    {
        _isHeld.OnChange += OnHeldChanged;
        _holderObject.OnChange += OnHolderChanged;
    }

    private void OnDisable()
    {
        _isHeld.OnChange -= OnHeldChanged;
        _holderObject.OnChange -= OnHolderChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _rigidbody.isKinematic = !IsServerStarted;
    }

    public bool CanInteract(PlayerHand hand)
    {
        if (_isHeld.Value)
            return false;

        if (!hand.IsEmpty)
            return false;

        if (!_data.IsReusable && _usesLeft <= 0)
            return false;

        return CanInteractInternal(hand);
    }

    public void RequestInteract(PlayerHand hand)
    {
        RequestPickupServerRpc(hand.NetworkObject);
    }

    public void RequestUse(Vector3 throwDirection, float throwForce)
    {
        RequestUseServerRpc(throwDirection, throwForce);
    }

    public void RequestDrop()
    {
        RequestDropServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(NetworkObject holderNetworkObject, NetworkConnection sender = null)
    {
        if (holderNetworkObject == null)
            return;

        var hand = holderNetworkObject.GetComponentInChildren<PlayerHand>();

        if (hand == null || !CanInteract(hand))
            return;

        _isHeld.Value = true;
        _holderObject.Value = holderNetworkObject;
        _holderConnection = sender;

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.isKinematic = true;
        _physicalCollider.isTrigger = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestUseServerRpc(Vector3 direction, float force, NetworkConnection sender = null)
    {
        if (!_isHeld.Value || _holderConnection != sender)
            return;

        if (_data.MaxUses > 0)
            _usesLeft--;

        ReleaseFromHand();

        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = direction.normalized * force;

        OnUsedServer();
        ObserversRpcUsedEffect();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestDropServerRpc(NetworkConnection sender = null)
    {
        if (!_isHeld.Value || _holderConnection != sender)
            return;

        ReleaseFromHand();
        _rigidbody.isKinematic = false;
        OnDroppedServer();
    }

    private void ReleaseFromHand()
    {
        _isHeld.Value = false;
        _holderObject.Value = null;
        _holderConnection = null;
        _physicalCollider.isTrigger = false;
    }

    private void OnHeldChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (newValue)
            _outline.enabled = false;

        if (!newValue)
        {
            NetworkObject.UnsetParent();

            if (_networkTransform != null)
                _networkTransform.enabled = true;

            if (!IsServerStarted)
                _rigidbody.isKinematic = true;
        }
        else
        {
            if (_networkTransform != null)
                _networkTransform.enabled = false;
        }
    }

    private void OnHolderChanged(NetworkObject oldValue, NetworkObject newValue, bool asServer)
    {
        if (oldValue != null)
        {
            var previousHand = oldValue.GetComponentInChildren<PlayerHand>();
            previousHand?.Clear();
        }

        if (newValue == null)
            return;

        var hand = newValue.GetComponentInChildren<PlayerHand>();

        if (hand == null)
        {
            Debug.LogError("[TakeableItem] PlayerHand component not found in children of holder NetworkObject!");
            return;
        }

        if (hand.HoldPoint == null)
        {
            Debug.LogError("[TakeableItem] hand.HoldPoint is null — assign _holdPoint in PlayerHand inspector!");
            return;
        }

        _rigidbody.interpolation = RigidbodyInterpolation.None;

        NetworkObject.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        hand.Hold(this);
        OnPickedUp();
    }

    [ObserversRpc]
    private void ObserversRpcUsedEffect()
    {
        OnUsed();
    }

    public void SetHighlighted(bool isHighlighted) => _outline.enabled = isHighlighted;

    protected virtual bool CanInteractInternal(PlayerHand hand) => true;

    protected virtual void OnPickedUp()
    {
        _physicalCollider.isTrigger = true;
    }

    protected virtual void OnUsedServer() { }

    protected virtual void OnUsed() { }

    protected virtual void OnDroppedServer() { }
}