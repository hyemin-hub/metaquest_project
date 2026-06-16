using UnityEngine;

/// <summary>
/// 아이템 한 종류의 정보. (공, 폭탄, 산성 포션, 소총 등)
///
/// ScriptableObject라서, Unity에서 우클릭 → Create → Inventory → Item Data 로
/// 아이템마다 에셋을 하나씩 만들어 값만 채우면 됨. 코드 수정 필요 없음.
/// 예: "공" 아이템 = 이름 "공", icon = 공 아이콘 PNG, prefab = 공 던지는 프리팹.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName = "아이템";

    [Tooltip("슬롯에 표시할 2D 아이콘 (PNG 스프라이트)")]
    public Sprite icon;

    [Tooltip("슬롯에 입체로 띄울 3D 모델 (glb에서 만든 프리팹). 없으면 2D 아이콘만 씀.")]
    public GameObject model3D;

    [Tooltip("이 아이템을 던질 때 생성할 투사체 프리팹 (팀원 B 프리팹). 나중에 연결.")]
    public GameObject projectilePrefab;
}
