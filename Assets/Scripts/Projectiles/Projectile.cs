using UnityEngine;

/// <summary>
/// 모든 투사체의 부모 클래스.
/// 일반 공(밤송이), 폭탄, 유도탄 등은 이 클래스를 상속받아 구현.
///
/// 사용 방법:
/// 1. Sphere GameObject에 Rigidbody + SphereCollider + XR Grab Interactable 추가
/// 2. 이 스크립트(또는 자식 클래스)를 컴포넌트로 추가
/// 3. 프리팹화하여 Assets/Prefabs/Projectiles/ 에 저장
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("성벽 블록에 줄 데미지 (A의 WallBlock.TakeDamage로 전달)")]
    public float damage = 10f;

    [Tooltip("던지고 나서 자동 삭제까지의 시간 (초)")]
    public float lifeTime = 10f;

    [Tooltip("충돌 시 자기 자신을 파괴할지 (폭탄은 true, 일반 공은 false 가능)")]
    public bool destroyOnHit = false;

    protected Rigidbody rb;
    protected bool hasCollided = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        // 일정 시간 후 자동 삭제 (안 맞고 떨어진 공 정리용)
        Destroy(gameObject, lifeTime);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // 중복 충돌 방지
        if (hasCollided) return;
        hasCollided = true;

        // TODO: A가 WallBlock 클래스 만들면 주석 해제
        // var block = collision.gameObject.GetComponent<WallBlock>();
        // if (block != null)
        // {
        //     block.TakeDamage(damage);
        // }

        Debug.Log($"[Projectile] {gameObject.name}이(가) {collision.gameObject.name}에 충돌");

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }
}
