using UnityEngine;

/// <summary>
/// Wall(빈 부모)에 붙이는 스크립트.
/// 자식 큐브들의 Rigidbody를 자동으로 긁어와서 관리하고,
/// hp가 0이 되면 전체 큐브를 동시에 무너뜨린다.
///
/// 사용법:
///  1. 빈 오브젝트(Wall)를 만들고 그 안에 Tower(n) 덩어리들을 전부 자식으로 넣는다.
///  2. 이 스크립트를 Wall 하나에만 붙인다. (개별 Tower(n)에는 아무것도 안 붙임)
///  3. 큐브에 Rigidbody가 없으면 아래 autoAddRigidbody = true 로 두면 자동으로 붙여준다.
///  끝. cubes 배열을 손으로 채울 필요 없음.
/// </summary>
public class CubeManager : MonoBehaviour
{
    [Header("체력")]
    public float maxHp = 100f;
    public float hp = 100f;

    [Header("무너질 때")]
    [Tooltip("폭발 힘의 세기")]
    public float explosionForce = 300f;
    [Tooltip("폭발이 퍼지는 반경")]
    public float explosionRadius = 5f;
    [Tooltip("위로 띄우는 정도 (0이면 옆으로만 날아감)")]
    public float upwardModifier = 0.5f;
    [Tooltip("회전(빙글빙글) 세기")]
    public float torque = 50f;

    [Header("옵션")]
    [Tooltip("큐브에 Rigidbody가 없으면 자동으로 붙일지")]
    public bool autoAddRigidbody = true;

    // 자동으로 채워지는 큐브 목록. Inspector에서 손댈 필요 없음.
    Rigidbody[] cubes;

    bool collapsed = false;

    void Awake()
    {
        // 자식에 있는 모든 Rigidbody를 자동 수집.
        cubes = GetComponentsInChildren<Rigidbody>(true);

        // Rigidbody가 하나도 없다면(큐브에 안 붙어 있다면) 자동으로 붙여줌.
        if ((cubes == null || cubes.Length == 0) && autoAddRigidbody)
        {
            AddRigidbodiesToChildren();
            cubes = GetComponentsInChildren<Rigidbody>(true);
        }

        // 평소엔 안 무너지게 전부 고정(kinematic).
        foreach (Rigidbody rb in cubes)
        {
            if (rb == null) continue;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        hp = maxHp;

        Debug.Log($"[CubeManager] '{name}' 큐브 {cubes.Length}개 수집 완료. HP={hp}");
    }

    /// <summary>
    /// 공/투사체가 이 메서드를 호출해서 데미지를 준다.
    /// (팀원 A의 TowerHealth와 같은 이름 — 투사체 코드가 둘 다 똑같이 때릴 수 있음)
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (collapsed) return;

        hp -= damage;
        Debug.Log($"[CubeManager] '{name}' HP = {hp}");

        if (hp <= 0f)
        {
            Collapse();
        }
    }

    void Collapse()
    {
        collapsed = true;
        Debug.Log($"[CubeManager] '{name}' 무너짐!");

        // 부모-자식 관계를 끊어서 각 큐브가 독립적으로 날아가게 함.
        transform.DetachChildren();

        foreach (Rigidbody rb in cubes)
        {
            if (rb == null) continue;

            rb.isKinematic = false;
            rb.useGravity = true;

            // 전체 큐브가 동시에 물리 ON → 와르르.
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.Impulse);
        }
    }

    // 큐브 자식들에 Rigidbody가 없을 때 자동으로 붙여주는 헬퍼.
    void AddRigidbodiesToChildren()
    {
        // MeshRenderer가 있는(=눈에 보이는) 자식들을 큐브로 간주하고 Rigidbody를 붙임.
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);
        foreach (MeshRenderer mr in renderers)
        {
            GameObject go = mr.gameObject;

            // 콜라이더가 없으면 BoxCollider도 붙여줌(충돌/물리 위해 필요).
            if (go.GetComponent<Collider>() == null)
            {
                go.AddComponent<BoxCollider>();
            }

            if (go.GetComponent<Rigidbody>() == null)
            {
                Rigidbody rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
        Debug.Log($"[CubeManager] '{name}' 자식 {renderers.Length}개에 Rigidbody 자동 추가.");
    }

    // ── 테스트용: 에디터에서 우클릭 메뉴로 강제로 무너뜨려 보기 ──
    [ContextMenu("테스트: 지금 무너뜨리기")]
    void TestCollapse()
    {
        TakeDamage(99999f);
    }
}
