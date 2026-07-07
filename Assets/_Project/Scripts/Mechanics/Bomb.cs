using UnityEngine;

public class Bomb : TakeableItem
{
    [SerializeField] private float _explodeDelay = 3f;

    protected override void OnUsed()
    {
        Invoke(nameof(Explode), _explodeDelay);
    }

    private void Explode()
    {
        Destroy(gameObject);
    }
}