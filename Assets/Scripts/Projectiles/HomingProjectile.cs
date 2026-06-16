using UnityEngine;

/// <summary>
/// 유도 미사일 투사체. 가장 가까운 약점(WeakPoint 태그) 블록을 향해 휘어 날아감.
/// MoSCoW: Could Have.
/// </summary>
public class HomingProjectile : Projectile
{
    [Header("유도 설정")]
    [Tooltip("회전 속도 (도/초). 높을수록 급격히 휨")]
    public float turnRate = 90f;

    [Tooltip("탐지 범위 (m)")]
    public float searchRadius = 20f;

    [Tooltip("약점 블록의 태그 이름 (A와 협의)")]
    public string weakPointTag = "WeakPoint";

    [Tooltip("앞으로 나아가는 속도")]
    public float forwardSpeed = 12f;

    private Transform target;
    private float retargetCooldown = 0.3f;
    private float retargetTimer;

    protected override void Awake()
    {
        base.Awake();
        destroyOnHit = true;
    }

    void FixedUpdate()
    {
        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f || target == null)
        {
            target = FindNearestWeakPoint();
            retargetTimer = retargetCooldown;
        }

        if (target != null)
        {
            Vector3 desiredDir = (target.position - transform.position).normalized;
            Vector3 newDir = Vector3.RotateTowards(
                rb.linearVelocity.normalized.magnitude > 0.01f ? rb.linearVelocity.normalized : transform.forward,
                desiredDir,
                turnRate * Mathf.Deg2Rad * Time.fixedDeltaTime,
                0f
            );
            rb.linearVelocity = newDir * forwardSpeed;
            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }

    Transform FindNearestWeakPoint()
    {
        GameObject[] candidates;
        try
        {
            candidates = GameObject.FindGameObjectsWithTag(weakPointTag);
        }
        catch
        {
            // 태그 없으면 무시
            return null;
        }

        Transform nearest = null;
        float minDist = searchRadius;
        foreach (var c in candidates)
        {
            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = c.transform;
            }
        }
        return nearest;
    }
}
