using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;   // XR Grab 감지용

/// <summary>
/// 바닥에 놓인 아이템(glb 본체)에 붙이는 스크립트.
/// 손으로 집는 순간 → 아이템창 슬롯에 자동으로 올라오게 한다.
///
/// 지금 목표: "줍기 → 손에 들림 → 슬롯에 아이템 패널 올라옴" 확인.
/// (던지기/벽 데미지는 지금 신경 안 씀)
///
/// 붙이는 곳:
///  바닥 아이템 오브젝트(glb)에 붙임.
///  이 오브젝트엔 XR Grab Interactable + Collider + Rigidbody 도 있어야 손으로 집힘.
///
/// Inspector 연결:
///  - itemData      : 이 아이템이 무엇인지 (ItemData 에셋)
///  - inventoryUI   : 씬의 InventoryUI (슬롯에 넣을 대상)
///                    비워두면 자동으로 씬에서 찾음.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class PickupItem : MonoBehaviour
{
    [Header("이 아이템 정보")]
    public ItemData itemData;

    [Header("아이템창 (비우면 자동으로 찾음)")]
    public InventoryUI inventoryUI;

    [Tooltip("집으면 바닥 본체를 사라지게 할지 (true면 슬롯으로 '들어간' 느낌)")]
    public bool hideOnPickup = true;

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    bool pickedUp = false;

    void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // 씬에서 InventoryUI 자동으로 찾기 (Inspector에 안 넣었을 때)
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }
    }

    void OnEnable()
    {
        // 손으로 "집는 순간" 이벤트 등록
        grab.selectEntered.AddListener(OnGrabbed);
    }

    void OnDisable()
    {
        grab.selectEntered.RemoveListener(OnGrabbed);
    }

    // 손이 이 아이템을 집었을 때 호출됨
    void OnGrabbed(SelectEnterEventArgs args)
    {
        if (pickedUp) return;        // 한 번만
        pickedUp = true;

        Debug.Log($"[PickupItem] '{itemData.itemName}' 주움!");

        // 아이템창 슬롯에 추가
        if (inventoryUI != null && itemData != null)
        {
            bool added = inventoryUI.AddItem(itemData);
            if (!added)
                Debug.Log("[PickupItem] 슬롯이 꽉 차서 못 넣음");
        }
        else
        {
            Debug.LogWarning("[PickupItem] inventoryUI 또는 itemData가 비어있음! Inspector 확인");
        }

        // 바닥 본체 사라지게 (슬롯으로 들어간 느낌)
        if (hideOnPickup)
        {
            gameObject.SetActive(false);
        }
    }
}
