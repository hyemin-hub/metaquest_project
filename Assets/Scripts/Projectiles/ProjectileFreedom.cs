using UnityEngine;

/// <summary>
/// 발사된 무기가 자유롭게 움직이도록 보장.
/// 매 프레임:
/// - parent가 자기도 모르게 다시 붙으면 즉시 분리
/// - isKinematic이 켜지면 즉시 끔
/// - useGravity 꺼지면 켬
/// - Collider isTrigger 꺼짐 (충돌 보장)
/// </summary>
public class ProjectileFreedom : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        // Collider isTrigger 다 끄기 (충돌 잡히게)
        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
        {
            if (c != null) c.isTrigger = false;
        }
    }

    void LateUpdate()
    {
        // 1. 부모 강제 분리
        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }

        // 2. Rigidbody 상태 강제 보장 (gunMode면 중력 무시)
        if (rb != null)
        {
            if (rb.isKinematic) rb.isKinematic = false;
            if (rb.linearDamping > 0.01f) rb.linearDamping = 0f;
            if (rb.constraints != RigidbodyConstraints.None) rb.constraints = RigidbodyConstraints.None;
            // useGravity는 발사자가 설정한 그대로 (gunMode면 false)
        }
    }
}
