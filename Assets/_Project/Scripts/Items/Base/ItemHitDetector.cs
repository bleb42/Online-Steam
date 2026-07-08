using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemHitDetector : MonoBehaviour
{
    public event Action<Collider> HitDetected;

    private Collider _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        HitDetected?.Invoke(other);
    }
}