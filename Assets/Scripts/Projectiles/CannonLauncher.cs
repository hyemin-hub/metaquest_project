using UnityEngine;

/// <summary>
/// 대포(Cannon)에 부착하는 발사 시스템.
/// Quest 컨트롤러 트리거 또는 마우스 클릭/스페이스로 발사.
///
/// 사용법:
/// 1. 혜민이 만든 Cannon.glb 모델을 씬에 배치
/// 2. 대포 끝부분(포구)에 빈 GameObject "MuzzlePoint" 만들고 z축이 발사 방향
/// 3. Cannon 부모에 이 컴포넌트 추가
/// 4. muzzlePoint, projectilePrefab 인스펙터에서 설정
/// </summary>
public class CannonLauncher : MonoBehaviour
{
    [Header("발사 설정")]
    [Tooltip("발사 위치 (포구). z축 forward 방향으로 날아감")]
    public Transform muzzlePoint;

    [Tooltip("발사할 투사체 프리팹 (Projectile 또는 ExplosiveProjectile)")]
    public GameObject projectilePrefab;

    [Tooltip("발사 초기 속도 (m/s)")]
    public float muzzleVelocity = 18f;

    [Tooltip("발사 간격 (초)")]
    public float fireCooldown = 1.2f;

    [Header("효과 (선택)")]
    public GameObject muzzleFlashVFX;
    public AudioClip fireSFX;

    [Header("입력 (Quest 없을 때 테스트용)")]
    [Tooltip("스페이스/마우스 클릭으로 발사")]
    public bool allowKeyboardFire = true;

    private float lastFireTime;

    void Update()
    {
        if (!allowKeyboardFire) return;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    public void Fire()
    {
        if (Time.time - lastFireTime < fireCooldown) return;
        if (muzzlePoint == null || projectilePrefab == null)
        {
            Debug.LogWarning("[Cannon] muzzlePoint 또는 projectilePrefab 미설정");
            return;
        }

        lastFireTime = Time.time;

        var proj = Instantiate(projectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
        var rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = muzzlePoint.forward * muzzleVelocity;
        }

        if (muzzleFlashVFX != null) Instantiate(muzzleFlashVFX, muzzlePoint.position, muzzlePoint.rotation);
        if (fireSFX != null) AudioSource.PlayClipAtPoint(fireSFX, muzzlePoint.position);

        Debug.Log($"[Cannon] 발사! 방향: {muzzlePoint.forward}, 속도: {muzzleVelocity}");
    }
}
