using UnityEngine;
using UnityEngine.XR;          // 구버전 XR 입력 (처음 하는 사람용 — 설정 안 건드려도 됨)
using System.Collections.Generic;

/// <summary>
/// VR 아이템창(인벤토리 패널) 관리 스크립트.
///
/// 하는 일:
///  - 왼손 컨트롤러의 Y버튼을 누르면 패널을 열고/닫는다 (토글).
///  - 패널은 World Space Canvas로, 게임오브젝트를 켜고 끄는 방식.
///  - 아이템 추가/슬롯 표시 메서드를 제공 (바닥 줍기 코드가 호출).
///
/// 붙이는 곳: 씬 어딘가 빈 오브젝트 (예: "UISystem") 하나에 붙이면 됨.
///
/// Inspector 연결:
///  - panel : 열고 닫을 아이템창 패널(World Space Canvas의 자식 패널) 오브젝트
///  - slots : 슬롯 칸들 (InventorySlot 컴포넌트가 붙은 오브젝트들)
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("아이템창 패널 (켜고 끌 오브젝트)")]
    public GameObject panel;

    [Header("슬롯 칸들 (왼쪽부터 순서대로)")]
    public InventorySlot[] slots;

    [Header("여닫기 설정")]
    [Tooltip("어느 손의 버튼으로 열까")]
    public bool useLeftHand = true;
    [Tooltip("토글(누를 때마다 열림/닫힘) vs 홀드(누르는 동안만 열림)")]
    public bool toggleMode = true;

    [Header("아이템을 쥐는 손 (5번: 고르면 손에 들기)")]
    [Tooltip("아이템 고르면 이 위치(보통 오른손 컨트롤러)에서 glb가 생성됨")]
    public Transform handAnchor;

    // 지금 손에 들고 있는 아이템 오브젝트 (glb 인스턴스)
    GameObject heldObject;

    // 내부 상태
    bool isOpen = false;
    bool buttonWasPressed = false;   // 버튼이 "방금 눌린 순간"을 잡기 위한 이전 프레임 상태

    void Start()
    {
        // 시작할 땐 닫아둠.
        if (panel != null) panel.SetActive(false);
        isOpen = false;
    }

    void Update()
    {
        bool buttonPressed = ReadInventoryButton();

        if (toggleMode)
        {
            // 토글: 버튼이 "안 눌림 → 눌림"으로 바뀌는 순간에만 1번 반응.
            if (buttonPressed && !buttonWasPressed)
            {
                ToggleInventory();
            }
        }
        else
        {
            // 홀드: 누르고 있으면 열림, 떼면 닫힘.
            SetInventoryOpen(buttonPressed);
        }

        buttonWasPressed = buttonPressed;
    }

    // ── 버튼 읽기 (구버전 XR 입력) ──
    // Quest 컨트롤러: 왼손 Y버튼 / 오른손 B버튼 = secondaryButton
    //                 왼손 X버튼 / 오른손 A버튼 = primaryButton
    // Y버튼을 쓰려면 왼손 + secondaryButton.
    bool ReadInventoryButton()
    {
        XRNode node = useLeftHand ? XRNode.LeftHand : XRNode.RightHand;
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);

        if (device.isValid &&
            device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool pressed))
        {
            return pressed;
        }
        return false;
    }

    // ── 여닫기 ──
    public void ToggleInventory()
    {
        SetInventoryOpen(!isOpen);
    }

    public void SetInventoryOpen(bool open)
    {
        if (open == isOpen) return;   // 상태 같으면 무시
        isOpen = open;
        if (panel != null) panel.SetActive(open);
        Debug.Log($"[InventoryUI] 아이템창 {(open ? "열림" : "닫힘")}");
    }

    // ── 아이템 추가 (바닥에서 주운 아이템을 인벤토리에 넣을 때 호출) ──
    /// <summary>
    /// 빈 슬롯을 찾아 아이템을 넣는다. 넣었으면 true, 자리 없으면 false.
    /// </summary>
    public bool AddItem(ItemData item)
    {
        foreach (InventorySlot slot in slots)
        {
            if (slot != null && slot.IsEmpty)
            {
                slot.SetItem(item);
                Debug.Log($"[InventoryUI] '{item.itemName}' 슬롯에 추가됨");
                return true;
            }
        }
        Debug.Log("[InventoryUI] 슬롯이 꽉 참 — 추가 실패");
        return false;
    }

    // ── 슬롯이 클릭됐을 때 (각 슬롯의 버튼 OnClick이 호출) ──
    // 5번: 아이템을 고르면 손에 든다.
    public void OnSlotClicked(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty) return;

        ItemData item = slot.CurrentItem;
        Debug.Log($"[InventoryUI] '{item.itemName}' 선택됨 → 손에 들기");

        EquipToHand(item);   // 손에 glb 생성

        // 3번: 아이템을 손에 들었으니 그 슬롯은 숨김(비움) 처리
        slot.Clear();

        // 골랐으니 창 닫기 (원하면 이 줄 지워도 됨)
        SetInventoryOpen(false);
    }

    // ── 손에 아이템(glb) 들기 ──
    void EquipToHand(ItemData item)
    {
        if (handAnchor == null)
        {
            Debug.LogWarning("[InventoryUI] handAnchor가 비어있음! 손 위치를 Inspector에 연결하세요.");
            return;
        }
        if (item.projectilePrefab == null)
        {
            Debug.LogWarning($"[InventoryUI] '{item.itemName}'의 glb 프리팹(projectilePrefab)이 비어있음!");
            return;
        }

        // 이미 들고 있던 게 있으면 치우기 (한 손에 하나)
        if (heldObject != null) Destroy(heldObject);

        // 손 위치에 glb 생성
        heldObject = Instantiate(item.projectilePrefab, handAnchor);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;

        Debug.Log($"[InventoryUI] '{item.itemName}' 손에 들림");
    }
}
