using UnityEngine;

/// <summary>
/// ⚠️ 임시 클래스 — A의 WallBlock이 완성되면 교체해야 함.
/// 시연을 위해 큐브에 붙여서 HP / 파괴 동작을 보여줌.
///
/// 사용법:
/// 1. 큐브 GameObject에 Rigidbody + Collider 추가
/// 2. 이 컴포넌트 추가
/// 3. HP가 0이 되면 자동 삭제됨
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SimpleHealthBlock : MonoBehaviour
{
    [Header("체력")]
    public float maxHP = 30f;
    public float currentHP;

    [Header("파괴 시 효과")]
    public GameObject destroyVFX;
    public AudioClip destroySFX;

    [Header("시작 시 고정")]
    [Tooltip("시작 시 isKinematic으로 공중 고정. 첫 데미지 받으면 풀림")]
    public bool freezeUntilHit = true;

    private Rigidbody rb;
    private Color originalColor;
    private bool hasOriginalColor = false;

    void Awake()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody>();
        if (rb != null && freezeUntilHit)
        {
            rb.isKinematic = true;
        }

        // 원래 색 저장
        var renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            originalColor = renderer.material.color;
            hasOriginalColor = true;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        Debug.Log($"[Block] {gameObject.name} HP: {currentHP}/{maxHP}");

        // 첫 데미지 들어오는 순간 — 모든 블럭의 isKinematic 해제 (전체 물리 적용)
        // 이래야 아래 블럭이 부서지면 위 블럭도 자연스럽게 떨어짐
        UnfreezeAllBlocks();

        // HP에 따라 원래 색 → 빨강으로 그라데이션
        var renderer = GetComponent<Renderer>();
        if (renderer != null && hasOriginalColor)
        {
            float ratio = Mathf.Clamp01(currentHP / maxHP);
            renderer.material.color = Color.Lerp(new Color(0.6f, 0.1f, 0.1f), originalColor, ratio);
        }

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    static bool worldUnfrozen = false;

    static void UnfreezeAllBlocks()
    {
        if (worldUnfrozen) return;
        worldUnfrozen = true;

        var allBlocks = FindObjectsByType<SimpleHealthBlock>(FindObjectsSortMode.None);
        foreach (var b in allBlocks)
        {
            var brb = b.GetComponent<Rigidbody>();
            if (brb != null && brb.isKinematic)
            {
                brb.isKinematic = false;
            }
        }
    }

    /// <summary>레벨 전환 시 다음 레벨의 블럭을 다시 고정 상태로 만들기 위해 리셋.</summary>
    public static void ResetFreezeState()
    {
        worldUnfrozen = false;
    }

    void Die()
    {
        if (destroyVFX != null) Instantiate(destroyVFX, transform.position, Quaternion.identity);
        if (destroySFX != null) AudioSource.PlayClipAtPoint(destroySFX, transform.position);

        // GameManager에 알림 (있으면)
        var gm = FindFirstObjectByType<SimpleGameManager>();
        if (gm != null) gm.OnBlockDestroyed();

        Destroy(gameObject);
    }
}
