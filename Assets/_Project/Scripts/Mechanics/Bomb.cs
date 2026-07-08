using System.Collections;
using FishNet;
using UnityEngine;

public class Bomb : TakeableItem
{
    [SerializeField] private float _explodeDelay = 3f;

    private bool _fuseStarted;

    protected override void OnUsedServer()
    {
        if (_fuseStarted) 
            return;

        _fuseStarted = true;
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