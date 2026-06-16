using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템창의 슬롯 한 칸.
///
/// 붙이는 곳: 슬롯 프레임 오브젝트(Image가 있는 UI 오브젝트)에 붙임.
///
/// Inspector 연결:
///  - iconImage  : 슬롯 안에서 아이템 2D 아이콘을 보여줄 Image
///  - model3DAnchor : (선택) 입체 3D 모델을 띄울 빈 자리. 비워둬도 됨.
///
/// 클릭 처리:
///  이 슬롯 오브젝트에 UI Button을 붙이고, Button의 OnClick에
///  InventoryUI.OnSlotClicked 를 연결하면 누를 때 반응함.
///  (또는 아래 OnClicked()를 OnClick에 직접 연결해도 됨)
/// </summary>
public class InventorySlot : MonoBehaviour
{
    [Header("표시용")]
    public Image iconImage;            // 2D 아이콘 보여줄 곳
    public Transform model3DAnchor;    // (선택) 3D 모델 띄울 자리

    [Header("이 슬롯을 관리하는 InventoryUI")]
    public InventoryUI owner;

    // 현재 이 슬롯에 든 아이템 (없으면 null)
    public ItemData CurrentItem { get; private set; }
    public bool IsEmpty => CurrentItem == null;

    GameObject spawnedModel;   // 띄워둔 3D 모델 인스턴스

    void Start()
    {
        Clear();   // 시작은 빈 칸으로
    }

    public void SetItem(ItemData item)
    {
        CurrentItem = item;

        // 2D 아이콘 표시
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = (item.icon != null);
        }

        // 3D 모델이 있고 띄울 자리가 있으면 입체로 띄움
        if (item.model3D != null && model3DAnchor != null)
        {
            if (spawnedModel != null) Destroy(spawnedModel);
            spawnedModel = Instantiate(item.model3D, model3DAnchor);
            spawnedModel.transform.localPosition = Vector3.zero;
        }
    }

    public void Clear()
    {
        CurrentItem = null;
        if (iconImage != null) iconImage.enabled = false;
        if (spawnedModel != null) { Destroy(spawnedModel); spawnedModel = null; }
    }

    // UI Button의 OnClick에 연결하면 됨
    public void OnClicked()
    {
        if (owner != null) owner.OnSlotClicked(this);
    }
}
