using UnityEngine;

/// <summary>
/// 폭탄 투사체. 충돌 시 주변 블록에 폭발력을 부여하여 사방으로 날려버림.
/// Projectile을 상속받아 OnCollisionEnter만 override.
/// </summary>
public class ExplosiveProjectile : Projectile
{
    [Header("폭발 설정")]
    [Tooltip("폭발 영향 범위 (m)")]
    public float explosionRadius = 3f;

    [Tooltip("주변 Rigidbody에 가할 폭발력")]
    public float explosionForce = 700f;

    [Tooltip("위쪽으로 살짝 띄우는 힘 (자연스러운 폭발 연출)")]
    public float upwardModifier = 1f;

    [Header("이펙트")]
    [Tooltip("터질 때 생성할 불꽃/연기 파티클 프리팹")]
    public GameObject explosionVFX;

    [Tooltip("폭발음 (선택)")]
    public AudioClip explosionSFX;

    protected override void Awake()
    {
        base.Awake();
        destroyOnHit = true; // 폭탄은 무조건 부딪히면 사라짐
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        Vector3 explosionPos = transform.position;

        // 범위 내 모든 콜라이더 탐지
        Collider[] hits = Physics.OverlapSphere(explosionPos, explosionRadius);

        foreach (Collider hit in hits)
        {
            // 1. Rigidbody 있으면 폭발력 부여
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardModifier, ForceMode.Impulse);
            }

            // 2. TODO: WallBlock 있으면 데미지 부여 (A 작업 끝나면 주석 해제)
            // var block = hit.GetComponent<WallBlock>();
            // if (block != null) block.TakeDamage(damage);
        }

        // 이펙트
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, explosionPos, Quaternion.identity);
        }

        // 사운드
        if (explosionSFX != null)
        {
            AudioSource.PlayClipAtPoint(explosionSFX, explosionPos);
        }

        Debug.Log($"[ExplosiveProjectile] 폭발! 위치: {explosionPos}, 영향받은 오브젝트: {hits.Length}개");

        Destroy(gameObject);
    }

    // 에디터에서 폭발 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
