using UnityEngine;

/// <summary>
/// Quest 없이 에디터에서 물리 시스템을 테스트하기 위한 임시 스크립트.
/// Play 모드 시작 후 delay 초 뒤에 자동으로 throwDirection 방향으로 발사됨.
///
/// 사용법:
/// 1. Sphere(공 프리팹)에 이 컴포넌트 추가
/// 2. Play 모드 진입 → 2초 뒤 자동 발사
/// 3. 큐브 벽에 부딪혀 와르르 무너지는지 확인
///
/// ※ 실기기(Quest) 빌드 시에는 이 컴포넌트 제거하거나 비활성화
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AutoThrowTest : MonoBehaviour
{
    [Header("발사 설정")]
    [Tooltip("발사 방향 (정규화돼서 적용됨)")]
    public Vector3 throwDirection = new Vector3(0f, 0.3f, 1f);

    [Tooltip("발사 힘")]
    public float throwForce = 15f;

    [Tooltip("Play 시작 후 몇 초 뒤 발사할지")]
    public float delay = 2f;

    [Tooltip("스페이스/마우스 클릭으로 재발사 가능")]
    public bool allowManualThrow = true;

    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;

        Invoke(nameof(Throw), delay);
    }

    void Update()
    {
        // R 키로 리셋 (다시 자리로) — Old Input Manager 기준
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }

        // 스페이스로 즉시 재발사
        if (allowManualThrow && Input.GetKeyDown(KeyCode.Space))
        {
            ResetBall();
            Throw();
        }
    }

    void Throw()
    {
        if (rb == null) return;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);
        Debug.Log($"[AutoThrowTest] 공 발사! 방향: {throwDirection.normalized}, 힘: {throwForce}");
    }

    void ResetBall()
    {
        if (rb == null) return;
        transform.position = startPos;
        transform.rotation = startRot;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
