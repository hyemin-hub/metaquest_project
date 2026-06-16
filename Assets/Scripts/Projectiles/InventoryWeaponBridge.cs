using UnityEngine;

/// <summary>
/// D팀원의 InventoryUI와 B의 WeaponSpawner를 연결하는 다리.
///
/// 동작:
/// - 슬롯이 빈칸이 되는 순간 = 사용자가 그 무기를 선택한 순간
/// - 그 ItemData의 projectilePrefab을 WeaponSpawner.grabbableBasePrefab으로 설정
/// - WeaponSpawner.Spawn() 호출해서 새 무기 즉시 생성
///
/// 사용: InventoryUI 옆에 빈 GameObject 만들고 이 컴포넌트 추가 후
///       inventoryUI, weaponSpawner 슬롯에 드래그.
///       (또는 DemoSceneSetup이 자동으로 처리)
/// </summary>
public class InventoryWeaponBridge : MonoBehaviour
{
    [Header("연결할 시스템")]
    public InventoryUI inventoryUI;
    public WeaponSpawner weaponSpawner;

    private ItemData[] lastSlotItems;

    void Start()
    {
        if (inventoryUI == null) inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (weaponSpawner == null) weaponSpawner = FindFirstObjectByType<WeaponSpawner>();

        if (inventoryUI != null && inventoryUI.slots != null)
        {
            lastSlotItems = new ItemData[inventoryUI.slots.Length];
            for (int i = 0; i < inventoryUI.slots.Length; i++)
            {
                if (inventoryUI.slots[i] != null)
                    lastSlotItems[i] = inventoryUI.slots[i].CurrentItem;
            }
        }
    }

    void Update()
    {
        if (inventoryUI == null || inventoryUI.slots == null || weaponSpawner == null) return;
        if (lastSlotItems == null || lastSlotItems.Length != inventoryUI.slots.Length)
        {
            lastSlotItems = new ItemData[inventoryUI.slots.Length];
        }

        for (int i = 0; i < inventoryUI.slots.Length; i++)
        {
            var slot = inventoryUI.slots[i];
            if (slot == null) continue;

            ItemData prev = lastSlotItems[i];
            ItemData curr = slot.CurrentItem;

            // 슬롯이 비워진 순간 = 사용자가 선택해서 손에 들었음
            if (prev != null && curr == null)
            {
                if (prev.projectilePrefab != null)
                {
                    weaponSpawner.grabbableBasePrefab = prev.projectilePrefab;
                    weaponSpawner.Spawn();
                    Debug.Log($"[Bridge] '{prev.itemName}' → WeaponSpawner 무기 교체 + 즉시 spawn");
                }
                else
                {
                    Debug.LogWarning($"[Bridge] '{prev.itemName}'에 projectilePrefab 없음 — ItemData 인스펙터에서 본인 prefab 할당해줘");
                }
            }

            lastSlotItems[i] = curr;
        }
    }
}
