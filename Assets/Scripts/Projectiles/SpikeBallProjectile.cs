using UnityEngine;

/// <summary>
/// 가시 모닝스타 — 최강 무기.
/// 거대한 폭발 반경 + 높은 데미지 + 강한 폭발력.
/// 3발 제한.
/// </summary>
public class SpikeBallProjectile : ExplosiveProjectile
{
    protected override void Awake()
    {
        base.Awake();
        damage = 60f;
        explosionRadius = 5f;
        explosionForce = 1800f;
        upwardModifier = 1.5f;
        destroyOnHit = true;
    }
}
