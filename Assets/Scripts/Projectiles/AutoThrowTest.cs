using UnityEngine;

/// <summary>
/// Quest 없이 에디터에서 물리 시스템을 테스트하기 위한 임시 스크립트.
/// 스페이스/마우스 클릭으로 발사. 카메라 방향으로 날아감.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AutoThrowTest : MonoBehaviour
{
    [Header("발사 설정")]
    [Tooltip("발사 방향 (로컬 또는 월드)")]
    public Vector3 throwDirection = new Vector3(0f, 0.3f, 1f);

    [Tooltip("발사 힘")]
    public float throwForce = 15f;

    [Tooltip("시작 후 자동 발사까지 딜레이 (0이면 자동 발사 안 함)")]
    public float autoDelay = 0f;

    [Tooltip("스페이스/클릭으로 수동 재발사 허용")]
    public bool allowManualThrow = true;

    [Tooltip("시작 시 isKinematic으로 떠있게 함. VR Grab 사용 시 false 권장")]
    public bool freezeAtStart = true;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool wasThrown = false;

    /// <summary>발사 시 호출되는 콜백 (WeaponSpawner가 인벤토리 차감용으로 등록)</summary>
    public System.Action onThrown;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        // 발사 전까지 공중에 떠있게 (떨어지지 않음)
        // VR Grab 사용 시에는 false (잡을 수 없게 되니까)
        if (freezeAtStart) rb.isKinematic = true;

        if (autoDelay > 0f)
        {
            Invoke(nameof(Throw), autoDelay);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) ResetBall();

        if (!allowManualThrow) return;

        // 스페이스: 정면 발사 (기본 방향)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetBall();
            Throw();
        }

        // 마우스 좌클릭: 클릭한 위치로 정확히 조준 발사
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                ResetBall();
                Throw();
                return;
            }

            ResetBall();
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            Vector3 targetPoint;
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(25f);
            }

            // 폭탄 → 타겟 직선 방향
            Vector3 dir = (targetPoint - transform.position).normalized;

            // 거리에 따른 미세한 위쪽 보정 (중력 낙하 보정)
            float horizontalDist = new Vector2(
                targetPoint.x - transform.position.x,
                targetPoint.z - transform.position.z
            ).magnitude;
            // 보정 줄임 — 정확하게 클릭한 곳으로
            dir.y += Mathf.Lerp(0.0f, 0.12f, Mathf.Clamp01(horizontalDist / 10f));
            dir = dir.normalized;

            // 거리 비례 힘 (가까우면 약하게, 멀면 강하게)
            float adjustedForce = Mathf.Lerp(throwForce * 0.9f, throwForce * 1.6f, Mathf.Clamp01(horizontalDist / 12f));

            ThrowToward(dir, adjustedForce);
        }
    }

    public void Throw()
    {
        ThrowToward(throwDirection, throwForce);
    }

    public void ThrowToward(Vector3 direction, float force)
    {
        if (rb == null) return;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);

        if (!wasThrown)
        {
            wasThrown = true;
            onThrown?.Invoke();
        }
    }

    void ResetBall()
    {
        if (rb == null) return;
        transform.position = startPos;
        transform.rotation = startRot;
        // 떠있는 상태로 복귀
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
