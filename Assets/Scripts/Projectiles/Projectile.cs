using UnityEngine;

/// <summary>
/// 모든 투사체의 부모 클래스.
/// 일반 공(밤송이), 폭탄, 유도탄 등은 이 클래스를 상속받아 구현.
///
/// 사용 방법:
/// 1. Sphere GameObject에 Rigidbody + SphereCollider + XR Grab Interactable 추가
/// 2. 이 스크립트(또는 자식 클래스)를 컴포넌트로 추가
/// 3. 프리팹화하여 Assets/Prefabs/Projectiles/ 에 저장
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("성벽 블록에 줄 데미지 (A의 WallBlock.TakeDamage로 전달)")]
    public float damage = 10f;

    [Tooltip("던지고 나서 자동 삭제까지의 시간 (초)")]
    public float lifeTime = 30f;

    [Tooltip("충돌 시 자기 자신을 파괴할지 (폭탄은 true, 일반 공은 false 가능)")]
    public bool destroyOnHit = false;

    protected Rigidbody rb;
    protected bool hasCollided = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        // 1. CubeDamage (개별 큐브) — 큐브 자체에 직접 부착
        var cube = collision.gameObject.GetComponent<CubeDamage>();
        if (cube != null) cube.TakeDamage(damage);

        // 2. TowerHealth (A 시스템 — 부모 Wall에 부착, 전체 hp 관리)
        var tower = collision.gameObject.GetComponentInParent<TowerHealth>();
        if (tower != null) tower.TakeDamage(damage);

        // 3. CubeManager (B 시스템 — 부모 Wall에 부착)
        var mgr = collision.gameObject.GetComponentInParent<CubeManager>();
        if (mgr != null) mgr.TakeDamage(damage);

        // 4. SimpleHealthBlock (시연용 임시)
        var block = collision.gameObject.GetComponent<SimpleHealthBlock>();
        if (block != null) block.TakeDamage(damage);

        Debug.Log($"[Projectile] {gameObject.name} → {collision.gameObject.name} 충돌, 데미지: {damage}");

        // 시각 피드백 — 맞은 큐브 빨강 깜빡임
        FlashCubeRed(collision.gameObject);

        if (destroyOnHit) Destroy(gameObject);
    }

    void FlashCubeRed(GameObject target)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer == null) return;

        // Material 인스턴스화
        var mat = renderer.material;
        var originalColor = mat.color;
        mat.color = Color.red;

        // 0.2초 후 원래 색
        var flasher = target.GetComponent<HitFlasher>();
        if (flasher == null) flasher = target.AddComponent<HitFlasher>();
        flasher.StartFlash(mat, originalColor);
    }
}

public class HitFlasher : MonoBehaviour
{
    Material mat;
    Color originalColor;
    float remaining;

    public void StartFlash(Material m, Color orig)
    {
        mat = m;
        originalColor = orig;
        remaining = 0.2f;
    }

    void Update()
    {
        if (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            if (remaining <= 0f && mat != null)
            {
                mat.color = originalColor;
            }
        }
    }
}
