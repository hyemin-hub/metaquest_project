using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 허리춤 인벤토리 벨트. 슬롯에 투사체 프리팹을 보관하고, 손이 가까이 가면 꺼내줌.
/// MoSCoW: Should Have.
///
/// 사용법:
/// 1. XR Origin 자식으로 빈 GameObject 만들고 허리 위치(0, 0.9, 0)에 배치
/// 2. 이 컴포넌트 추가
/// 3. slotPositions에 슬롯 Transform들 등록 (벨트 양 옆에 빈 GameObject)
/// 4. availableProjectiles에 던질 수 있는 공 프리팹들 등록
/// </summary>
public class InventoryBelt : MonoBehaviour
{
    [Header("슬롯 / 프리팹")]
    [Tooltip("벨트 슬롯 위치들 (빈 GameObject)")]
    public List<Transform> slotPositions = new List<Transform>();

    [Tooltip("벨트에 채울 투사체 프리팹들 (순서대로 슬롯에 배정)")]
    public List<GameObject> availableProjectiles = new List<GameObject>();

    [Header("동작 설정")]
    [Tooltip("손이 이 거리 안으로 들어오면 공 꺼내줌")]
    public float pickupRadius = 0.15f;

    [Tooltip("같은 슬롯이 다시 채워지는 쿨다운")]
    public float refillCooldown = 1.5f;

    private GameObject[] slotInstances;
    private float[] slotCooldowns;

    void Start()
    {
        slotInstances = new GameObject[slotPositions.Count];
        slotCooldowns = new float[slotPositions.Count];
        RefillAll();
    }

    void Update()
    {
        // 슬롯 쿨다운 진행
        for (int i = 0; i < slotInstances.Length; i++)
        {
            if (slotInstances[i] == null && slotCooldowns[i] > 0f)
            {
                slotCooldowns[i] -= Time.deltaTime;
                if (slotCooldowns[i] <= 0f)
                {
                    Refill(i);
                }
            }
        }
    }

    void RefillAll()
    {
        for (int i = 0; i < slotPositions.Count; i++)
        {
            Refill(i);
        }
    }

    void Refill(int slotIdx)
    {
        if (availableProjectiles.Count == 0) return;
        var prefab = availableProjectiles[slotIdx % availableProjectiles.Count];
        if (prefab == null) return;

        var instance = Instantiate(prefab, slotPositions[slotIdx].position, slotPositions[slotIdx].rotation);
        instance.transform.SetParent(slotPositions[slotIdx], worldPositionStays: true);

        // 슬롯에 있을 때는 물리 끄기 (꺼낼 때 다시 켜짐)
        var rb = instance.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        slotInstances[slotIdx] = instance;
    }

    /// <summary>
    /// 슬롯에서 공을 꺼냈다고 알려주는 함수 (XR Grab 이벤트에서 호출 권장).
    /// </summary>
    public void NotifyPickup(int slotIdx)
    {
        if (slotIdx < 0 || slotIdx >= slotInstances.Length) return;
        var rb = slotInstances[slotIdx]?.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        slotInstances[slotIdx] = null;
        slotCooldowns[slotIdx] = refillCooldown;
    }
}
