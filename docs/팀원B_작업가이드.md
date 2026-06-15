# 🎯 팀원 B 작업 가이드 — VR 인터랙션 / 투사체

> 회의 자료 + MoSCoW 표 기반으로 본인이 무엇을 어떤 순서로 만들어야 하는지 정리한 문서.

---

## 📌 한눈에 (TL;DR)

| 항목 | 내용 |
|------|------|
| 담당 | VR 인터랙션 / 투사체 시스템 |
| 작업 씬 | `Assets/Scenes/Test_B_Scene.unity` |
| 만들 스크립트 | `Projectile.cs` (부모) → `ExplosiveProjectile.cs`, `HomingProjectile.cs` 등 (자식) |
| 만들 프리팹 | `Assets/Prefabs/Projectiles/` 아래 (일반공, 폭탄, 유도 등) |
| 푸시할 것 | **스크립트 + 프리팹만** (※ MainScene 절대 손대지 말기) |
| 핵심 Unity 기능 | XR Interaction Toolkit, Rigidbody, OnCollisionEnter, AddExplosionForce |

---

## 🗂️ 만들 파일 목록

### 스크립트 (`Assets/Scripts/Projectiles/`)

| 파일 | 우선순위 | 역할 |
|------|------|------|
| `Projectile.cs` | **Must** | 모든 투사체의 부모 (충돌 감지, 데미지 전달) |
| `ExplosiveProjectile.cs` | **Should** | 폭탄 — 충돌 시 주변 블록에 폭발력 부여 |
| `HomingProjectile.cs` | **Could** | 유도 미사일 — 약점 블록으로 휘어 날아감 |
| `InventoryBelt.cs` | **Should** | 허리춤 인벤토리 — 구체 보관/꺼내기 |

### 프리팹 (`Assets/Prefabs/Projectiles/`)

| 프리팹 | 우선순위 | 베이스 |
|------|------|------|
| `Projectile_Basic.prefab` (밤송이) | **Must** | `Projectile.cs` |
| `Projectile_Bomb.prefab` (폭탄) | **Should** | `ExplosiveProjectile.cs` |
| `Projectile_Homing.prefab` (유도탄) | **Could** | `HomingProjectile.cs` |

---

## 🚦 작업 우선순위 (스프린트 순서대로)

### 🔴 1단계 — Must Have (이게 먼저)

> 목표: "VR로 구체를 집어 던지면 성벽 블록에 부딪힌다"가 동작하는 것.

- [ ] `Test_B_Scene` 열고 XR Origin (Quest 컨트롤러) 세팅
- [ ] 간단한 큐브 블록 하나 놓고 (A의 성벽 프리팹 받기 전까지 임시)
- [ ] `Projectile.cs` 작성 → 일반 구체 프리팹에 붙이기
- [ ] XR Grab Interactable로 손으로 집고 던지는 기능 동작 확인
- [ ] 던진 공이 큐브에 맞으면 `OnCollisionEnter` 작동 확인 (Debug.Log라도)
- [ ] 투사체 가능 횟수 제한 (예: 무제한 / 10발 등 — `GameManager`가 관리할 변수)

### 🟡 2단계 — Should Have

> 목표: 폭탄 던지면 주변 블록이 사방으로 날아간다 + 허리춤에서 공 꺼내기.

- [ ] `ExplosiveProjectile.cs` 작성 (Projectile 상속)
- [ ] 폭탄 프리팹 만들기 + 파티클(불꽃/연기) 붙이기
- [ ] `Physics.OverlapSphere`로 주변 블록 감지 → `AddExplosionForce`
- [ ] `InventoryBelt.cs` — 허리 위치에 슬롯 만들고 구체 보관/꺼내기

### 🟢 3단계 — Could Have

- [ ] `HomingProjectile.cs` — 가장 가까운 약점 블록(빨간 태그)으로 휘어 날아감
- [ ] 가챠/꽝 투사체 (C의 GachaManager랑 연동)

### ⚪ 4단계 — Would Have (안 함)

- 산성 액체병, 자석 기믹 등은 이번 스프린트 X

---

## 🛠️ 핵심 기능별 가이드

### 1. XR Grab으로 공 집기 (Must)

**준비:**
1. `Test_B_Scene`에 `XR Origin (VR)` 추가 (`GameObject → XR → XR Origin (VR)`)
2. 카메라 설정 → Tracking Origin Mode: **Floor**
3. 컨트롤러에 `XR Direct Interactor` 또는 `XR Ray Interactor` 추가

**공 프리팹 세팅:**
1. 빈 GameObject → Sphere 메시 추가
2. **컴포넌트 추가:**
   - `Rigidbody` (Use Gravity ✅, Mass 적당히)
   - `Sphere Collider`
   - `XR Grab Interactable` ← 이게 핵심
   - `Projectile` 스크립트 (다음 단계에서 만들 거)

> XR Grab Interactable이 붙으면 자동으로 손으로 집고 던지는 게 됨. 트리거 떼면 손 속도가 Rigidbody에 적용돼서 자연스럽게 날아감.

### 2. `Projectile.cs` (Must)

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    [Header("기본 설정")]
    public float damage = 10f;          // 블록에 줄 데미지
    public float lifeTime = 10f;        // 자동 삭제 시간

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifeTime);   // 안 맞고 떨어진 공은 일정 시간 후 제거
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // A가 만든 WallBlock에 데미지 주기
        var block = collision.gameObject.GetComponent<WallBlock>();
        if (block != null)
        {
            block.TakeDamage(damage);
        }

        // 일반 공은 한번 부딪히면 사라지게 (원하면)
        // Destroy(gameObject);
    }
}
```

> ⚠️ `WallBlock`은 A가 만드는 클래스. 아직 없으면 컴파일 에러 나니까 임시로 `Debug.Log("맞음")` 정도로 시작하고, A가 push 한 뒤 연동.

### 3. `ExplosiveProjectile.cs` (Should)

```csharp
using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    [Header("폭발 설정")]
    public float explosionRadius = 3f;   // 영향 범위
    public float explosionForce = 700f;  // 밀어내는 힘
    public GameObject explosionVFX;      // 불꽃/연기 파티클 프리팹

    protected override void OnCollisionEnter(Collision collision)
    {
        // 폭발 위치
        Vector3 explosionPos = transform.position;

        // 주변 콜라이더 다 모으기
        Collider[] hits = Physics.OverlapSphere(explosionPos, explosionRadius);

        foreach (Collider hit in hits)
        {
            // Rigidbody 있는 애들한테만 힘 부여
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null)
            {
                hitRb.AddExplosionForce(explosionForce, explosionPos, explosionRadius);
            }

            // 데미지도 부여
            var block = hit.GetComponent<WallBlock>();
            if (block != null) block.TakeDamage(damage);
        }

        // 이펙트 띄우고 자기 자신 제거
        if (explosionVFX != null) Instantiate(explosionVFX, explosionPos, Quaternion.identity);
        Destroy(gameObject);
    }
}
```

> 💡 `explosionRadius`, `explosionForce`는 인스펙터에서 튜닝. 처음엔 너무 크면 맵이 다 날아가니까 작게 시작.

### 4. 인벤토리 벨트 (Should)

핵심 아이디어: 허리 위치에 빈 GameObject 슬롯 4~6개 두고, 손이 그 위치에 오면 새 공을 `Instantiate`해서 손에 쥐어주는 식.

- `XR Socket Interactor`를 쓰거나
- 직접 `OnTriggerEnter`로 손 감지 후 `Instantiate`

이건 좀 무거운 기능이라 Must 끝나고 시작해도 됨.

### 5. 유도 미사일 (Could)

- 약점 블록 태그를 `WeakPoint`로 잡고 (A랑 협의)
- `FixedUpdate`에서 가장 가까운 `WeakPoint` 찾아서 `Vector3.RotateTowards`로 진행 방향 살짝 휘게

---

## 🤝 다른 팀원과의 인터페이스 (협의 필수)

### A (성벽) 와 협의할 것
- `WallBlock` 클래스에 **`public void TakeDamage(float damage)`** 메서드가 있어야 함 → A에게 요청
- 블록에 `Rigidbody` 붙어있어야 폭발력이 먹힘 → A에게 확인
- 약점 블록 태그명 통일 (예: `WeakPoint`)

### C (게임 매니저) 와 협의할 것
- 투사체 가능 횟수는 `GameManager`가 관리 → 던질 때마다 `GameManager.Instance.UseProjectile()` 같은 거 호출
- 가챠 시스템: C의 `GachaManager`가 본인 프리팹들을 배열에 등록해서 랜덤 생성

### D (UI/통합) 에게 전달할 것
- 완성된 공 프리팹들 (`Projectile_Basic`, `Projectile_Bomb` 등) → D가 MainScene에서 가챠 매니저 배열에 드래그
- 현재 남은 투사체 개수 → D의 UI에 표시될 변수

---

## ✅ 작업 시작 전 체크리스트

- [ ] `git pull origin main` 했음 (최신 받기)
- [ ] Unity Package Manager에서 **Meta XR All-in-One SDK** 설치 (필수!)
  - `Window → Package Manager → +` → `com.meta.xr.sdk.all`
- [ ] `Test_B_Scene` 열어서 본인 작업 시작
- [ ] 새 브랜치: `git checkout -b feature/B-basic-projectile`
- [ ] **MainScene 절대 건드리지 않기**

## ✅ 커밋 전 체크리스트

- [ ] Test_B_Scene에서 동작 확인 (에디터 Play 모드 또는 Quest 빌드)
- [ ] 스크립트 `.cs` + `.cs.meta` 둘 다 커밋됨
- [ ] 프리팹 `.prefab` + `.prefab.meta` 둘 다 커밋됨
- [ ] `Test_B_Scene.unity`의 변경사항은 **커밋하지 말기** (개인 테스트용이라)
  - 또는 본인 테스트용 변경은 OK이지만 다른 팀원 작업이랑 충돌 안 나는지 체크
- [ ] **MainScene 안 건드림**

---

## 📅 추천 일정 (참고용)

| 주차 | 작업 |
|------|------|
| 1주차 | XR 환경 세팅 + 임시 큐브로 던지기 동작 확인 |
| 2주차 | `Projectile.cs` 완성 + 일반 공 프리팹 + A의 WallBlock 연동 |
| 3주차 | `ExplosiveProjectile.cs` + 폭탄 프리팹 + 파티클 |
| 4주차 | 인벤토리 벨트 (Should) |
| 5주차+ | 유도 미사일, 가챠 연동 (Could) |

---

## 🔗 참고 자료

- [Unity XR Interaction Toolkit — Grab Interactable](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/xr-grab-interactable.html)
- [Rigidbody.AddExplosionForce 공식 문서](https://docs.unity3d.com/ScriptReference/Rigidbody.AddExplosionForce.html)
- [Physics.OverlapSphere 공식 문서](https://docs.unity3d.com/ScriptReference/Physics.OverlapSphere.html)
- [Meta XR SDK — Interaction](https://developer.oculus.com/documentation/unity/unity-isdk-interaction-sdk-overview/)

---

## 🆘 막히면

- XR 세팅 문제 → hyemin (인프라 담당)
- 성벽 안 부서지는데? → 팀원 A
- 매니저 / 가챠 연동 → 팀원 C
- 최종 통합 / 빌드 에러 → 팀원 D
