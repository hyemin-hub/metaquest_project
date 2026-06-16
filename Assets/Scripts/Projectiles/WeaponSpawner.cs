using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 무기 스폰 시스템 — WeaponInventory의 currentWeapon에 따라 적절한 투사체 생성.
/// 시작 시 첫 무기 자동 스폰, 사용 후 자동 재스폰.
/// R 키로 즉시 재스폰.
/// </summary>
public class WeaponSpawner : MonoBehaviour
{
    [Header("스폰 위치")]
    public Vector3 spawnPosition = new Vector3(0, 1.8f, -8);

    [Header("카메라 추적 (사용자 이동 시 spawn 따라오게)")]
    [Tooltip("켜면 매 프레임 카메라 앞으로 spawn 위치 업데이트")]
    public bool followCamera = true;

    [Tooltip("카메라에서 얼마나 앞에 spawn할지")]
    public Vector3 cameraOffset = new Vector3(0.3f, -0.3f, 1f);

    [Header("동작")]
    public bool spawnOnStart = false; // 자동 spawn 끔 (트리거로만 생성)
    public KeyCode respawnKey = KeyCode.R;
    public float autoRespawnDelay = 0f; // 자동 재스폰 끔

    [Header("총처럼 - 트리거 누르면 생성+자동발사")]
    public bool gunMode = true;
    public float gunModeThrowForce = 800f; // scale 32 환경 — 매우 강하게
    public Vector3 gunModeAngleBoost = new Vector3(0, 0.05f, 0);
    [Tooltip("중력 무시하고 직선으로 날아가게 (성벽까지 곧장)")]
    public bool gunModeNoGravity = true;

    [Header("발사 설정")]
    public Vector3 throwDirection = new Vector3(0, 0.5f, 1);
    public float throwForce = 32f;

    [Header("Grab 베이스 프리팹 (Meta XR Building Block 큐브)")]
    [Tooltip("Building Block으로 만든 Grabbable 큐브 프리팹. 이게 있으면 무기가 이걸 베이스로 만들어져서 VR에서 잡을 수 있음")]
    public GameObject grabbableBasePrefab;

    [Header("공 크기 배율 (카메라 scale에 맞게 조정)")]
    [Tooltip("prefab 사용 시 원본 scale에 이 값 곱함")]
    public float weaponScaleMultiplier = 20f;

    [Header("무기 자동 정리 (0이면 끔)")]
    [Tooltip("이 거리(m) 이상 멀어지면 destroy. scale 25 환경에선 500 이상 권장")]
    public float maxDistanceBeforeDestroy = 1000f;

    private GameObject currentProjectile;
    private float lastDestroyedTime = -999f;

    void Start()
    {
        // 시연용 강제 설정 — Inspector 값 무시하고 보장
        gunMode = true;
        spawnOnStart = false;
        autoRespawnDelay = 0f;

        // prefab 미할당 시 씬에서 [BuildingBlock] Cube 자동 찾기
        if (grabbableBasePrefab == null)
        {
            var bbCube = GameObject.Find("[BuildingBlock] Cube");
            if (bbCube != null)
            {
                grabbableBasePrefab = bbCube;
                bbCube.SetActive(false);
                Debug.Log("[WeaponSpawner] [BuildingBlock] Cube 자동 base 설정");
            }
        }

        Debug.Log($"[WeaponSpawner] Start — gunMode={gunMode}, spawnOnStart={spawnOnStart}");
    }

    private bool triggerWasPressed = false;

    void Update()
    {
        // 카메라 추적 — 사용자가 VR에서 이동하면 spawn 위치도 따라옴
        if (followCamera)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 newSpawn = cam.transform.position
                    + cam.transform.right * cameraOffset.x
                    + cam.transform.up * cameraOffset.y
                    + cam.transform.forward * cameraOffset.z;
                spawnPosition = newSpawn;
            }
        }

        // 총 모드 — 트리거 누르면 즉시 생성 + 카메라 방향 발사 (VRInputHelper로 robust)
        if (gunMode)
        {
            bool triggerNow = VRInputHelper.IsTriggerPressed(0.5f);

            // 키보드 F 키도 가능 (에디터 테스트)
            if (Input.GetKey(KeyCode.F)) triggerNow = true;

            if (triggerNow && !triggerWasPressed)
            {
                SpawnAndShoot();
            }
            triggerWasPressed = triggerNow;
        }

        if (Input.GetKeyDown(respawnKey)) Spawn();

        // 너무 멀리 가버린 무기 자동 destroy (scale 25 환경 고려 충분히 멀리)
        if (currentProjectile != null && maxDistanceBeforeDestroy > 0)
        {
            float dist = Vector3.Distance(currentProjectile.transform.position, spawnPosition);
            if (dist > maxDistanceBeforeDestroy)
            {
                Debug.Log($"[WeaponSpawner] 무기 {dist:F1}m 멀어짐 → 재스폰");
                Destroy(currentProjectile);
                currentProjectile = null;
                lastDestroyedTime = Time.time;
            }
        }

        if (autoRespawnDelay > 0f && currentProjectile == null)
        {
            if (Time.time - lastDestroyedTime > autoRespawnDelay)
            {
                Spawn();
            }
        }
    }

    // 총 모드 — 무기 생성 + 카메라 방향으로 즉시 발사
    public void SpawnAndShoot()
    {
        SpawnInternal();
        if (currentProjectile == null) return;

        var bullet = currentProjectile;

        // ⭐ 강제 부모 분리
        bullet.transform.SetParent(null, true);

        // Grab/Hand/Pose/Interact 관련 모든 컴포넌트 비활성화
        DisableInteractionComponents(bullet);

        // 자식 GameObject들 안의 Grab 컴포넌트도 비활성화
        foreach (var t in bullet.GetComponentsInChildren<Transform>())
        {
            if (t == bullet.transform) continue;
            DisableInteractionComponents(t.gameObject);
        }

        // 모든 Collider isTrigger 끄기 (충돌 잡히게)
        foreach (var col in bullet.GetComponentsInChildren<Collider>())
        {
            if (col != null) col.isTrigger = false;
        }

        // ⭐ bullet 본체에 SphereCollider 강제 추가 (충돌 무조건 잡히게)
        if (bullet.GetComponent<Collider>() == null)
        {
            var sc = bullet.AddComponent<SphereCollider>();
            sc.radius = 0.5f; // 큰 반경으로 확실히 잡힘
            sc.isTrigger = false;
            Debug.Log("[WeaponSpawner] SphereCollider 강제 추가");
        }

        // ⭐ Projectile 컴포넌트 강제 보장 (없으면 추가)
        if (bullet.GetComponent<Projectile>() == null)
        {
            var p = bullet.AddComponent<Projectile>();
            p.damage = 150f;
            p.destroyOnHit = false;
        }

        // ⭐ Raycast 충돌 감지 (무기 빠를 때 통과 방지)
        if (bullet.GetComponent<ProjectileRaycastDamage>() == null)
        {
            var rd = bullet.AddComponent<ProjectileRaycastDamage>();
            rd.damage = 150f;
            rd.radius = 1f; // 큰 반경으로 확실히 잡힘
        }

        var rb = bullet.GetComponent<Rigidbody>();
        if (rb == null) rb = bullet.AddComponent<Rigidbody>();

        rb.isKinematic = false;
        rb.useGravity = !gunModeNoGravity; // gunMode면 중력 무시
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // ⭐ Drag/공기저항 강제 0
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
        rb.mass = 1f;

        // ⭐ Constraint도 다 풀기 (회전/이동 제약 X)
        rb.constraints = RigidbodyConstraints.None;

        Camera cam = Camera.main;
        Vector3 dir = cam != null ? cam.transform.forward + gunModeAngleBoost : transform.forward + gunModeAngleBoost;
        rb.linearVelocity = dir.normalized * gunModeThrowForce;
        rb.angularVelocity = Random.insideUnitSphere * 5f;

        // ⭐ ProjectileFreedom 추가 — 매 프레임 강제 보장
        if (bullet.GetComponent<ProjectileFreedom>() == null)
        {
            bullet.AddComponent<ProjectileFreedom>();
        }

        Debug.Log($"[WeaponSpawner] 총 발사! 속도: {gunModeThrowForce}, 방향: {dir.normalized}");

        // 발사된 무기는 currentProjectile에서 분리 (새 무기 또 생성 가능하게)
        currentProjectile = null;
    }

    void DisableInteractionComponents(GameObject go)
    {
        foreach (var comp in go.GetComponents<MonoBehaviour>())
        {
            if (comp == null) continue;
            string name = comp.GetType().Name;
            if (name.Contains("Grab") || name.Contains("Hand") ||
                name.Contains("Pose") || name.Contains("Interact") ||
                name.Contains("Pointable") || name.Contains("Snap"))
            {
                comp.enabled = false;
            }
        }
    }

    public void Spawn()
    {
        // gunMode면 자동 spawn 막음 (트리거로만 SpawnAndShoot 호출)
        if (gunMode)
        {
            Debug.Log("[WeaponSpawner] gunMode 활성 — Spawn() 호출 무시. 트리거 누르면 SpawnAndShoot");
            return;
        }
        SpawnInternal();
    }

    void SpawnInternal()
    {
        if (currentProjectile != null) Destroy(currentProjectile);

        var inv = WeaponInventory.Instance;
        WeaponType type = inv != null ? inv.currentWeapon : WeaponType.Pebble;

        // 보유 횟수 없으면 자동으로 다른 무기 선택
        if (inv != null && !inv.CanUse(type))
        {
            // 우선순위: Pebble > Bomb > SpikeBall
            if (inv.CanUse(WeaponType.Pebble)) type = WeaponType.Pebble;
            else if (inv.CanUse(WeaponType.Bomb)) type = WeaponType.Bomb;
            else if (inv.CanUse(WeaponType.SpikeBall)) type = WeaponType.SpikeBall;
            inv.SelectWeapon(type);
        }

        currentProjectile = CreateProjectile(type);
        lastDestroyedTime = Time.time;
    }

    GameObject CreateProjectile(WeaponType type)
    {
        GameObject go;
        bool isFromPrefab = grabbableBasePrefab != null;

        if (isFromPrefab)
        {
            // Building Block의 Grab 컴포넌트 다 살아있는 prefab/씬오브젝트 사용
            go = Instantiate(grabbableBasePrefab, spawnPosition, Quaternion.identity);
            go.SetActive(true);

            // ⭐ 부모 분리 — 각 무기가 독립적으로 움직이게
            go.transform.SetParent(null, true);

            // 크기 배율 적용 — prefab의 원본 scale * 배율 (무기마다 다른 크기 가능)
            if (weaponScaleMultiplier > 0.01f && weaponScaleMultiplier != 1f)
            {
                Vector3 prefabScale = grabbableBasePrefab.transform.localScale;
                // prefab scale이 (0,0,0)이면 fallback (1,1,1) 사용
                if (prefabScale.sqrMagnitude < 0.001f) prefabScale = Vector3.one;
                go.transform.localScale = prefabScale * weaponScaleMultiplier;
            }
        }
        else
        {
            // Fallback: 기본 Sphere
            go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = spawnPosition;
        }

        // Rigidbody 보장 + 던지기 가능하게 설정
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = go.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }
        // VR Grab 후 던지기 가능하도록 (isKinematic이면 던져도 velocity 적용 안 됨)
        rb.isKinematic = false;
        rb.useGravity = true;
        // CCD 적용 (큐브 통과 방지)
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // ForceReleaseFix 추가 — 자연스러운 throw 시도
        if (isFromPrefab && go.GetComponent<ForceReleaseFix>() == null)
        {
            go.AddComponent<ForceReleaseFix>();
        }

        // QuestTriggerThrow 추가 — Quest 컨트롤러 트리거/AB 버튼으로 명시 발사
        if (isFromPrefab && go.GetComponent<QuestTriggerThrow>() == null)
        {
            var trigger = go.AddComponent<QuestTriggerThrow>();
            trigger.throwForce = throwForce;
        }

        var throwTest = go.AddComponent<AutoThrowTest>();
        throwTest.throwDirection = throwDirection;
        throwTest.throwForce = throwForce;
        throwTest.autoDelay = 0f;
        throwTest.allowManualThrow = !isFromPrefab; // VR Grab 쓸 땐 키보드 발사 끔
        throwTest.freezeAtStart = !isFromPrefab;   // Grab Prefab은 isKinematic 안 걸기
        throwTest.onThrown = OnProjectileThrown;

        // 렌더러 찾기 (prefab에 자식으로 있을 수도)
        var rend = go.GetComponent<Renderer>() ?? go.GetComponentInChildren<Renderer>();
        Material mat = null;
        if (rend != null && rend.sharedMaterial != null)
        {
            mat = new Material(rend.sharedMaterial);
        }

        // prefab 사용 시 스케일 변경 안 함 (Grab 인터랙션 기준점 흐트러질 수 있음)
        bool resizeScale = !isFromPrefab;

        switch (type)
        {
            case WeaponType.Pebble:
                go.name = "Weapon_Pebble";
                if (resizeScale) go.transform.localScale = Vector3.one * 0.35f;
                if (mat != null) mat.color = new Color(0.15f, 0.15f, 0.15f); // 검은색 (8번공)
                var pebbleProj = go.AddComponent<Projectile>();
                pebbleProj.damage = 10f;
                pebbleProj.destroyOnHit = false;
                break;

            case WeaponType.Bomb:
                go.name = "Weapon_Bomb";
                if (resizeScale) go.transform.localScale = Vector3.one * 0.5f;
                if (mat != null) mat.color = new Color(0.95f, 0.15f, 0.15f); // 빨강
                var bomb = go.AddComponent<ExplosiveProjectile>();
                bomb.damage = 25f;
                bomb.explosionRadius = 3f;
                bomb.explosionForce = 1000f;
                AddTrail(go, new Color(1f, 0.5f, 0.1f, 1f));
                AddPointLight(go, new Color(1f, 0.4f, 0.1f), 3f);
                break;

            case WeaponType.SpikeBall:
                go.name = "Weapon_SpikeBall";
                if (resizeScale) go.transform.localScale = Vector3.one * 0.7f;
                if (mat != null) mat.color = new Color(0.25f, 0.28f, 0.4f); // 청회색 (모닝스타)
                go.AddComponent<SpikeBallProjectile>();
                if (!isFromPrefab) AddSpikes(go); // prefab은 큐브라서 가시 안 붙임
                AddTrail(go, new Color(0.6f, 0.3f, 1f, 1f));
                AddPointLight(go, new Color(0.5f, 0.4f, 1f), 4f);
                break;
        }

        if (rend != null && mat != null) rend.material = mat;

        return go;
    }

    void OnProjectileThrown()
    {
        // 던지면 인벤토리 차감
        if (WeaponInventory.Instance != null)
        {
            WeaponInventory.Instance.TryUseCurrent();
        }
    }

    void AddTrail(GameObject target, Color startColor)
    {
        var trail = target.AddComponent<TrailRenderer>();
        trail.time = 0.4f;
        trail.startWidth = 0.3f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        var trailMat = new Material(Shader.Find("Sprites/Default"));
        trail.material = trailMat;
        trail.startColor = startColor;
        var endColor = startColor;
        endColor.a = 0f;
        trail.endColor = endColor;
    }

    void AddPointLight(GameObject target, Color color, float intensity)
    {
        var lightGO = new GameObject("ProjLight");
        lightGO.transform.SetParent(target.transform, false);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = 5f;
    }

    void AddSpikes(GameObject target)
    {
        // 6방향 가시
        Vector3[] dirs = {
            Vector3.up, Vector3.down,
            Vector3.left, Vector3.right,
            Vector3.forward, Vector3.back
        };

        foreach (var d in dirs)
        {
            var spike = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Destroy(spike.GetComponent<Collider>()); // 가시 콜라이더는 제거 (스피어 콜라이더로 통일)
            spike.transform.SetParent(target.transform, false);
            spike.transform.localPosition = d * 0.6f;
            spike.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
            spike.transform.up = d;

            var spikeRend = spike.GetComponent<Renderer>();
            if (spikeRend != null)
            {
                var mat = new Material(spikeRend.sharedMaterial);
                mat.color = new Color(0.85f, 0.85f, 0.9f); // 은색
                spikeRend.material = mat;
            }
        }
    }
}
