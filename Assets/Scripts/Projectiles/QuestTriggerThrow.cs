using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Meta Quest 컨트롤러로 강제 발사.
/// 잡고 있는 상태에서 트리거 버튼(검지) 누르면 카메라 forward 방향으로 발사.
/// 또는 A/X 버튼으로도 발사.
///
/// 키보드 T로도 발사 가능 (에디터 테스트용).
/// </summary>
public class QuestTriggerThrow : MonoBehaviour
{
    [Header("발사 설정")]
    public float throwForce = 100f; // scale 25 환경 — 더 강하게
    public Vector3 throwAngleBoost = new Vector3(0, 0.3f, 0);

    [Header("입력 옵션")]
    public bool useTriggerButton = true;
    public bool useAB_Button = false; // A/B는 무기 전환용 (WeaponInventory)
    public bool useKeyboard = true;

    private Rigidbody rb;
    private bool hasThrown = false;
    private float lastThrowTime = -999f;
    private float throwCooldown = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Time.time - lastThrowTime < throwCooldown) return;

        bool triggerPressed = false;

        if (useTriggerButton)
        {
            // OpenXR 호환 — 양손 검지 트리거 (trigger axis)
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            var leftHand = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            float rTrigger = 0f, lTrigger = 0f;
            if (rightHand.isValid) rightHand.TryGetFeatureValue(CommonUsages.trigger, out rTrigger);
            if (leftHand.isValid) leftHand.TryGetFeatureValue(CommonUsages.trigger, out lTrigger);
            if (rTrigger > 0.7f || lTrigger > 0.7f) triggerPressed = true;
        }

        if (useKeyboard && Input.GetKeyDown(KeyCode.T))
        {
            triggerPressed = true;
        }

        if (triggerPressed)
        {
            ThrowForward();
        }
    }

    public void ThrowForward()
    {
        if (rb == null) return;

        // 1. Grabbable 관련 모든 컴포넌트 비활성화 (손 트래킹 끊기)
        DisableGrabComponents();

        // 2. 부모 끊기 (손이 부모일 가능성)
        if (transform.parent != null) transform.SetParent(null, true);

        // 3. Rigidbody 강제 활성화
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 4. 발사 방향 계산
        Camera cam = Camera.main;
        Vector3 dir = cam != null ? cam.transform.forward + throwAngleBoost : transform.forward + throwAngleBoost;
        dir = dir.normalized;

        // 5. 카메라 앞쪽으로 살짝 이동 (Grabbable 손 위치에서 벗어나게)
        transform.position += dir * 0.4f;

        // 6. velocity 적용
        rb.linearVelocity = dir * throwForce;
        rb.angularVelocity = Random.insideUnitSphere * 5f;

        lastThrowTime = Time.time;
        hasThrown = true;
        Debug.Log($"[QuestTriggerThrow] 트리거 발사! 방향: {dir}, 속도: {throwForce}");

        // 30초 후 자동 정리
        Destroy(gameObject, 30f);
    }

    void DisableGrabComponents()
    {
        // 이름에 Grab 포함된 모든 MonoBehaviour 비활성화
        var components = GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp == null || comp == this) continue;
            string typeName = comp.GetType().Name;
            if (typeName.Contains("Grab") || typeName.Contains("Hand") ||
                typeName.Contains("Pose") || typeName.Contains("Interact"))
            {
                comp.enabled = false;
            }
        }

        // 자식 GameObject들도 (HandGrabPoint 등)
        foreach (var t in GetComponentsInChildren<Transform>())
        {
            if (t == transform) continue;
            var childComps = t.GetComponents<MonoBehaviour>();
            foreach (var comp in childComps)
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                if (typeName.Contains("Grab") || typeName.Contains("Hand") ||
                    typeName.Contains("Pose"))
                {
                    comp.enabled = false;
                }
            }
        }
    }
}
