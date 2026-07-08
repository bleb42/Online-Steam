public interface IInteractable
{
    void SetHighlighted(bool isHighlighted);
    bool CanInteract(PlayerHand hand);
    void RequestInteract(PlayerHand hand);
}