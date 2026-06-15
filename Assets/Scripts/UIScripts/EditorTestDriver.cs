using UnityEngine;

/// <summary>
/// ★ 에디터 테스트 전용 ★ (VR 없이 키보드/마우스로 확인)
/// 나중에 VR 붙일 때 이 스크립트만 떼면 됨.
///
/// 붙이는 곳: UISystem(InventoryUI 붙은 오브젝트)에 같이 붙이면 편함.
///
/// 조작:
///  - I 키         : 아이템창 열기/닫기 (VR의 Y버튼 대신)
///  - 1~5 숫자키   : testItems에 넣어둔 아이템을 "주운 것처럼" 슬롯에 추가
///  - 마우스 클릭  : 슬롯을 직접 클릭하면 InventorySlot의 Button이 반응함
///                   (Canvas에 일반 Graphic Raycaster가 있어야 에디터에서 마우스 클릭됨)
/// </summary>
public class EditorTestDriver : MonoBehaviour
{
    [Header("연결")]
    public InventoryUI inventoryUI;     // 비우면 자동으로 찾음

    [Header("테스트로 주울 아이템들 (1~5키에 대응)")]
    public ItemData[] testItems;        // 만들어둔 ItemData 5개를 여기 드래그

    void Awake()
    {
        if (inventoryUI == null)
            inventoryUI = Object.FindFirstObjectByType<InventoryUI>();
    }

    void Update()
    {
        if (inventoryUI == null) return;

        // I 키 = 창 열기/닫기 (VR Y버튼 대신)
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.ToggleInventory();
            Debug.Log("[테스트] I키 → 아이템창 토글");
        }

        // 1~5 키 = 해당 아이템을 주운 것처럼 슬롯에 추가
        CheckPickupKey(KeyCode.Alpha1, 0);
        CheckPickupKey(KeyCode.Alpha2, 1);
        CheckPickupKey(KeyCode.Alpha3, 2);
        CheckPickupKey(KeyCode.Alpha4, 3);
        CheckPickupKey(KeyCode.Alpha5, 4);
    }

    void CheckPickupKey(KeyCode key, int index)
    {
        if (!Input.GetKeyDown(key)) return;
        if (testItems == null || index >= testItems.Length || testItems[index] == null)
        {
            Debug.Log($"[테스트] {index + 1}번 자리에 ItemData가 비어있음");
            return;
        }
        bool added = inventoryUI.AddItem(testItems[index]);
        Debug.Log($"[테스트] {index + 1}키 → '{testItems[index].itemName}' 줍기 {(added ? "성공" : "실패(슬롯 꽉참)")}");
    }
}
