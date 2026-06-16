using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Collections.Generic;

/// <summary>
/// B팀원 자체 인벤토리 UI (D팀 코드 의존 X).
///
/// - 왼손 Y버튼으로 토글 (구버전 XR 입력 사용)
/// - 슬롯 6개 직접 관리
/// - 슬롯 클릭 → WeaponSpawner.grabbableBasePrefab 교체 + 재spawn
///
/// 사용: 빈 GameObject에 추가 → Inspector에서 panel/buttons/icons/slots 채우거나
///       DemoSceneSetup이 자동으로 처리.
/// </summary>
public class CustomInventoryUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotData
    {
        public string itemName = "Item";
        public Sprite icon;
        public GameObject prefab; // 슬롯 선택 시 WeaponSpawner에 적용할 prefab
    }

    [Header("패널 (켜고 끌 오브젝트)")]
    public GameObject panel;

    [Header("UI 버튼/아이콘 (씬에서 생성 후 연결)")]
    public Button[] slotButtons = new Button[6];
    public Image[] slotIcons = new Image[6];

    [Header("슬롯별 데이터 (6개)")]
    public SlotData[] slots = new SlotData[6];

    [Header("연결")]
    public WeaponSpawner weaponSpawner;

    [Header("입력 옵션")]
    public bool useLeftHand = true;
    public bool toggleMode = true;
    public bool startOpen = true; // 시작부터 열려있게 (테스트용)

    private bool isOpen = false;
    private bool buttonWasPressed = false;
    private int currentSlotIndex = 0;

    void Start()
    {
        if (panel != null) panel.SetActive(startOpen);
        isOpen = startOpen;

        if (weaponSpawner == null) weaponSpawner = FindFirstObjectByType<WeaponSpawner>();

        // 슬롯 버튼 → 이벤트 연결 + 아이콘 표시
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;
            if (slotButtons[i] != null)
            {
                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(idx));
            }

            if (slotIcons[i] != null && i < slots.Length && slots[i].icon != null)
            {
                slotIcons[i].sprite = slots[i].icon;
                slotIcons[i].enabled = true;
                slotIcons[i].preserveAspect = true;
            }
            else if (slotIcons[i] != null)
            {
                slotIcons[i].enabled = false;
            }
        }

        // 시작 시 첫 번째 슬롯의 무기 자동 선택 (큐브 대신 진짜 무기 보이게)
        Invoke(nameof(SelectFirstSlot), 0.3f);
    }

    void SelectFirstSlot()
    {
        if (slots != null && slots.Length > 0 && slots[0] != null && slots[0].prefab != null)
        {
            currentSlotIndex = 0;
            if (weaponSpawner != null)
            {
                weaponSpawner.grabbableBasePrefab = slots[0].prefab;
                // gunMode면 spawn 안 함 (트리거로만 발사) — prefab만 교체
                if (!weaponSpawner.gunMode) weaponSpawner.Spawn();
                Debug.Log($"[CustomInventoryUI] 시작 시 첫 무기 자동 선택: {slots[0].itemName}");
            }
        }
    }

    [Header("카메라 따라가기 (꺼두기 — 위치 고정)")]
    public bool followCamera = false; // 인벤토리는 월드 고정. 사용자가 잡은 위치 유지
    [Tooltip("X=좌우, Y=위아래(-가 아래), Z=앞")]
    public Vector3 cameraOffset = new Vector3(0, -0.6f, 0.9f);
    [Tooltip("카메라 기준 각도 (X = 아래로 기울이기)")]
    public Vector3 lookDownAngle = new Vector3(45, 0, 0);

    void LateUpdate()
    {
        // 인벤토리 Canvas가 항상 카메라 앞에 보이게
        if (followCamera && panel != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 targetPos = cam.transform.position
                    + cam.transform.right * cameraOffset.x
                    + cam.transform.up * cameraOffset.y
                    + cam.transform.forward * cameraOffset.z;

                // Canvas (panel의 부모) 자체를 옮기기 — 시야 하단에 살짝 기울여서
                Transform canvasT = panel.transform.parent;
                if (canvasT != null)
                {
                    canvasT.position = targetPos;
                    canvasT.rotation = cam.transform.rotation * Quaternion.Euler(lookDownAngle);
                }
            }
        }
    }

    void Update()
    {
        bool pressed = ReadInventoryButton();

        if (toggleMode)
        {
            if (pressed && !buttonWasPressed) ToggleInventory();
        }
        else
        {
            SetInventoryOpen(pressed);
        }

        buttonWasPressed = pressed;

        // VR 컨트롤러 A/B 버튼 — OpenXR 호환 (UnityEngine.XR)
        ReadCycleButtons();

        // 키보드 [ ] 키 (에디터 테스트용)
        if (Input.GetKeyDown(KeyCode.RightBracket)) CycleSlot(+1);
        if (Input.GetKeyDown(KeyCode.LeftBracket)) CycleSlot(-1);
    }

    public void CycleSlot(int direction)
    {
        if (slots == null || slots.Length == 0) return;
        currentSlotIndex = ((currentSlotIndex + direction) % slots.Length + slots.Length) % slots.Length;
        Debug.Log($"[CustomInventoryUI] 슬롯 사이클 → {currentSlotIndex} ({slots[currentSlotIndex].itemName})");
        OnSlotClicked(currentSlotIndex);
    }

    private bool aWasPressed = false;
    private bool bWasPressed = false;

    void ReadCycleButtons()
    {
        // VRInputHelper로 robust (OVRInput + UnityEngine.XR 둘 다 체크)
        bool aNow = VRInputHelper.IsAButtonPressed();
        bool bNow = VRInputHelper.IsBButtonPressed();

        if (aNow && !aWasPressed) CycleSlot(+1);
        if (bNow && !bWasPressed) CycleSlot(-1);

        aWasPressed = aNow;
        bWasPressed = bNow;
    }

    bool ReadInventoryButton()
    {
        // VRInputHelper — OVRInput + UnityEngine.XR 동시 체크
        if (useLeftHand && VRInputHelper.IsYButtonPressed()) return true;
        if (!useLeftHand && VRInputHelper.IsBButtonPressed()) return true;

        // 키보드 백업 (에디터)
        if (Input.GetKey(KeyCode.I)) return true;
        return false;
    }

    public void ToggleInventory()
    {
        SetInventoryOpen(!isOpen);
    }

    public void SetInventoryOpen(bool open)
    {
        if (open == isOpen) return;
        isOpen = open;
        if (panel != null) panel.SetActive(open);
        Debug.Log($"[CustomInventoryUI] 인벤토리 {(open ? "열림" : "닫힘")}");
    }

    public void OnSlotClicked(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        var s = slots[index];
        if (s == null) return;

        Debug.Log($"[CustomInventoryUI] 슬롯 {index} ('{s.itemName}') 선택");

        if (weaponSpawner != null && s.prefab != null)
        {
            weaponSpawner.grabbableBasePrefab = s.prefab;
            // gunMode일 때는 spawn 안 함 (트리거 누를 때만)
            if (!weaponSpawner.gunMode) weaponSpawner.Spawn();
            Debug.Log($"[CustomInventoryUI] 무기 교체 → '{s.itemName}' (gunMode={weaponSpawner.gunMode})");
        }
        else if (s.prefab == null)
        {
            Debug.LogWarning($"[CustomInventoryUI] 슬롯 {index} ('{s.itemName}')에 prefab 없음");
        }

        SetInventoryOpen(false);
    }
}
