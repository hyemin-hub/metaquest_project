using UnityEngine;

/// <summary>
/// VR에서 컨트롤러를 휘둘러서 던지면 자동으로 throw 발동.
/// 매 프레임 큐브 위치 기록 → 평균 속도가 throwSpeedThreshold 넘으면 throw.
/// </summary>
public class ForceReleaseFix : MonoBehaviour
{
    [Header("자동 Throw 설정")]
    [Tooltip("이 속도(m/s) 넘게 휘두르면 자동 발사 — 낮을수록 살짝 휘둘러도 발사")]
    public float throwSpeedThreshold = 0.3f; // 1.5 → 0.3 (아주 살짝 흔들어도 발사)

    [Tooltip("발사 속도 배율 (휘두른 속도 * 이 값)")]
    public float velocityBoost = 8f; // 2.5 → 8 (scale 25 환경에서 더 멀리)

    [Tooltip("발사 후 재발사 방지 시간")]
    public float throwCooldown = 0.6f;

    private Rigidbody rb;
    private Vector3[] positionHistory = new Vector3[6];
    private int historyIndex = 0;
    private bool initialized = false;
    private float lastThrowTime = -999f;
    private bool hasThrown = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (rb == null || hasThrown) return;

        if (!initialized)
        {
            for (int i = 0; i < positionHistory.Length; i++)
                positionHistory[i] = transform.position;
            initialized = true;
            return;
        }

        // 매 프레임 위치 기록
        positionHistory[historyIndex] = transform.position;
        historyIndex = (historyIndex + 1) % positionHistory.Length;

        if (Time.time - lastThrowTime < throwCooldown) return;

        // 평균 속도 계산
        Vector3 totalDelta = Vector3.zero;
        int oldIdx = historyIndex;
        for (int i = 1; i < positionHistory.Length; i++)
        {
            int curr = (oldIdx + i) % positionHistory.Length;
            int prev = (oldIdx + i - 1) % positionHistory.Length;
            totalDelta += positionHistory[curr] - positionHistory[prev];
        }
        Vector3 avgVel = totalDelta / (positionHistory.Length - 1) / Time.fixedDeltaTime;

        // 빠른 휘두르기 감지 → 자동 throw
        if (avgVel.magnitude > throwSpeedThreshold)
        {
            ExecuteThrow(avgVel * velocityBoost);
        }

        // useGravity 강제
        if (!rb.useGravity && !rb.isKinematic) rb.useGravity = true;
    }

    void ExecuteThrow(Vector3 throwVelocity)
    {
        // 1. Grab 컴포넌트 모두 비활성화
        DisableGrabComponents();

        // 2. 부모 끊기
        if (transform.parent != null) transform.SetParent(null, true);

        // 3. Rigidbody 활성화
        rb.isKinematic = false;
        rb.useGravity = true;

        // 4. velocity 적용
        rb.linearVelocity = throwVelocity;
        rb.angularVelocity = Random.insideUnitSphere * 5f;

        lastThrowTime = Time.time;
        hasThrown = true;

        Debug.Log($"[ForceReleaseFix] 휘두르기 throw! 속도: {throwVelocity.magnitude:F1} m/s");

        // 30초 후 자동 destroy (성벽 닿기 충분한 시간)
        Destroy(gameObject, 30f);
    }

    void DisableGrabComponents()
    {
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
