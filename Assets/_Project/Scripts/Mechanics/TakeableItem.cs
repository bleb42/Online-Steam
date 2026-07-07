using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Outline))]
public abstract class TakeableItem : MonoBehaviour, ITakeable
{
    [SerializeField] protected ItemData _data;
    [SerializeField] protected Collider _physicalCollider;

    protected Rigidbody _rigidbody;
    protected Outline _outline;
    protected PlayerHand _hand;

    private int _usesLeft;
    private RigidbodyInterpolation _defaultInterpolation;

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _outline = GetComponent<Outline>();
        _outline.enabled = false;
        _usesLeft = _data.MaxUses;
        _defaultInterpolation = _rigidbody.interpolation;
    }

    public bool CanInteract(PlayerHand hand)
    {
        if (!hand.IsEmpty)
            return false;

        if (!_data.IsReusable && _usesLeft <= 0)
            return false;

        return CanInteractInternal(hand);
    }

    public void Interact(PlayerHand hand)
    {
        _hand = hand;

        _rigidbody.interpolation = RigidbodyInterpolation.None;
        _rigidbody.isKinematic = true;
        _physicalCollider.isTrigger = true;

        transform.SetParent(hand.transform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        hand.Hold(this);
        OnPickedUp();
    }

    public void Use()
    {
        if (_data.MaxUses > 0)
            _usesLeft--;

        Detach();
        OnUsed();
    }

    public void Drop()
    {
        Detach();
        OnDropped();
    }

    public void SetHighlighted(bool isHighlighted) => _outline.enabled = isHighlighted;

    private void Detach()
    {
        transform.SetParent(null);
        _physicalCollider.isTrigger = false;
        _rigidbody.isKinematic = false;
        _rigidbody.interpolation = _defaultInterpolation;
        _hand = null;
    }

    protected virtual bool CanInteractInternal(PlayerHand hand) => true;
    protected virtual void OnPickedUp() { }
    protected virtual void OnUsed() { }
    protected virtual void OnDropped() { }
}