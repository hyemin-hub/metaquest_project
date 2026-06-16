using UnityEngine;

/// <summary>
/// Raycast로 매 프레임 충돌 감지 + 데미지 적용.
/// 무기가 아무리 빨라도 한 프레임 안에 이동한 경로 전체를 raycast로 검사하므로 통과 안 함.
/// </summary>
public class ProjectileRaycastDamage : MonoBehaviour
{
    public float damage = 100f;
    public float radius = 0.5f;
    public bool destroyOnHit = false;

    private Vector3 lastPos;
    private bool initialized = false;
    private bool hasHit = false;

    void Start()
    {
        lastPos = transform.position;
        initialized = true;
    }

    void FixedUpdate()
    {
        if (!initialized || hasHit) return;

        Vector3 currentPos = transform.position;
        Vector3 diff = currentPos - lastPos;
        float dist = diff.magnitude;

        if (dist > 0.01f)
        {
            // SphereCast로 경로 위 모든 콜라이더 감지
            RaycastHit[] hits = Physics.SphereCastAll(lastPos, radius, diff.normalized, dist);
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                // 자기 자신 제외
                if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)) continue;

                DoDamage(hit.collider.gameObject);
                if (destroyOnHit)
                {
                    hasHit = true;
                    Destroy(gameObject, 0.1f);
                    return;
                }
            }
        }

        lastPos = currentPos;
    }

    void DoDamage(GameObject target)
    {
        bool hit = false;

        var cube = target.GetComponent<CubeDamage>();
        if (cube != null) { cube.TakeDamage(damage); hit = true; }

        var tower = target.GetComponentInParent<TowerHealth>();
        if (tower != null) { tower.TakeDamage(damage); hit = true; }

        var mgr = target.GetComponentInParent<CubeManager>();
        if (mgr != null) { mgr.TakeDamage(damage); hit = true; }

        var block = target.GetComponent<SimpleHealthBlock>();
        if (block != null) { block.TakeDamage(damage); hit = true; }

        if (hit)
        {
            Debug.Log($"[RaycastDamage] {target.name}에 {damage} 데미지 (raycast)");
            FlashRed(target);
        }
    }

    void FlashRed(GameObject target)
    {
        var rend = target.GetComponent<Renderer>();
        if (rend == null) return;
        var mat = rend.material;
        var origColor = mat.color;
        mat.color = Color.red;
        var f = target.GetComponent<HitFlasher>();
        if (f == null) f = target.AddComponent<HitFlasher>();
        f.StartFlash(mat, origColor);
    }
}
