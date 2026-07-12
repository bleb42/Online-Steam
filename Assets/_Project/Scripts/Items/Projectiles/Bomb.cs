using System.Collections;
using FishNet;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class Bomb : TakeableItem
{
    [SerializeField] private float _explodeDelay = 3f;

    private readonly SyncVar<bool> _fuseStarted = new SyncVar<bool>();

    protected override bool CanInteractInternal(PlayerHand hand)
    {
        return !_fuseStarted.Value;
    }

    protected override void OnUsedServer()
    {
        if (_fuseStarted.Value)
            return;

        _fuseStarted.Value = true;
        StartCoroutine(FuseCoroutine());
    }

    private IEnumerator FuseCoroutine()
    {
        yield return new WaitForSeconds(_explodeDelay);

        Explode();
    }

    private void Explode()
    {
        ObserversRpcExplode(transform.position);
        InstanceFinder.ServerManager.Despawn(gameObject);
    }

    [FishNet.Object.ObserversRpc]
    private void ObserversRpcExplode(Vector3 position)
    {
        Debug.Log($"[Bomb] BOOM at {position}");
    }
}