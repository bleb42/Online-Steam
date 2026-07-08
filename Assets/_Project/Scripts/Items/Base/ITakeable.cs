using UnityEngine;

public interface ITakeable : IInteractable
{
    void RequestUse(Vector3 throwDirection, float throwForce);
    void RequestDrop();
}