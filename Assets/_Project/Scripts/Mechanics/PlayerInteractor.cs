using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerInteractor : NetworkBehaviour
{
    [SerializeField] private InputReader _inputReader;
    [SerializeField] private PlayerHand _hand;

    private readonly List<IInteractable> _nearby = new();
    private IInteractable _currentInteractable;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) 
            return;

        _inputReader.OnInteractPerformed += TryInteract;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (_inputReader == null) 
            return;

        _inputReader.OnInteractPerformed -= TryInteract;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) 
            return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            _nearby.Add(interactable);
            UpdateCurrent();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsOwner) return;

        if (other.TryGetComponent(out IInteractable interactable))
        {
            _nearby.Remove(interactable);
            interactable.SetHighlighted(false);
            UpdateCurrent();
        }
    }

    private void UpdateCurrent()
    {
        if (_currentInteractable != null)
            _currentInteractable.SetHighlighted(false);

        _currentInteractable = null;

        foreach (var interactable in _nearby)
        {
            if (interactable.CanInteract(_hand))
            {
                _currentInteractable = interactable;
                break;
            }
        }

        if (_currentInteractable != null)
            _currentInteractable.SetHighlighted(true);
    }

    private void TryInteract()
    {
        if (_currentInteractable == null)
            return;

        _currentInteractable.RequestInteract(_hand);
        UpdateCurrent();
    }
}