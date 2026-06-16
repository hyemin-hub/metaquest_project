using UnityEngine;

/// <summary>
/// 폭탄 스폰 시스템.
/// R 키 누르면 (또는 자동으로) 폭탄을 새로 생성해서 시연 가능.
/// </summary>
public class BombSpawner : MonoBehaviour
{
    [Header("스폰 위치")]
    public Vector3 spawnPosition = new Vector3(0, 1.5f, -6);

    [Header("스폰 동작")]
    [Tooltip("시작 시 자동으로 첫 폭탄 생성")]
    public bool spawnOnStart = true;

    [Tooltip("R 키 입력으로 폭탄 재생성")]
    public KeyCode respawnKey = KeyCode.R;

    [Tooltip("자동 재스폰 딜레이 (0이면 안 함, 1.5초 권장)")]
    public float autoRespawnDelay = 1.5f;

    [Header("폭탄 설정")]
    public float bombDamage = 15f;
    public float bombExplosionRadius = 2.5f;
    public float bombExplosionForce = 800f;
    public Vector3 throwDirection = new Vector3(0, 0.55f, 1);
    public float throwForce = 32f;

    private GameObject currentBomb;
    private float lastDestroyedTime = -999f;

    void Start()
    {
        if (spawnOnStart) Invoke(nameof(SpawnBomb), 0.1f);
    }

    void Update()
    {
        // 수동 재스폰
        if (Input.GetKeyDown(respawnKey))
        {
            SpawnBomb();
        }

        // 자동 재스폰 (폭탄이 사라지고 delay 지나면)
        if (autoRespawnDelay > 0f && currentBomb == null)
        {
            if (Time.time - lastDestroyedTime > autoRespawnDelay)
            {
                SpawnBomb();
            }
        }

        if (currentBomb == null && lastDestroyedTime < -100f)
        {
            // 처음 한 번 기록
            lastDestroyedTime = Time.time;
        }
        else if (currentBomb == null)
        {
            // 이미 사라진 상태 — lastDestroyedTime 이미 갱신됨
        }
    }

    public void SpawnBomb()
    {
        // 기존 폭탄 제거
        if (currentBomb != null) Destroy(currentBomb);

        // 새 폭탄 생성
        var bomb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bomb.name = "Demo_Bomb_Live";
        bomb.transform.position = spawnPosition;
        bomb.transform.localScale = Vector3.one * 0.5f;

        var rb = bomb.AddComponent<Rigidbody>();
        rb.mass = 1f;

        var explosive = bomb.AddComponent<ExplosiveProjectile>();
        explosive.damage = bombDamage;
        explosive.explosionRadius = bombExplosionRadius;
        explosive.explosionForce = bombExplosionForce;
        explosive.lifeTime = 30f;

        var throwTest = bomb.AddComponent<AutoThrowTest>();
        throwTest.throwDirection = throwDirection;
        throwTest.throwForce = throwForce;
        throwTest.autoDelay = 0f;
        throwTest.allowManualThrow = true;

        // 빨강 색 + 빛
        var rend = bomb.GetComponent<Renderer>();
        if (rend != null && rend.sharedMaterial != null)
        {
            var mat = new Material(rend.sharedMaterial);
            mat.color = new Color(0.95f, 0.15f, 0.15f);
            rend.material = mat;
        }

        // Trail
        var trail = bomb.AddComponent<TrailRenderer>();
        trail.time = 0.4f;
        trail.startWidth = 0.3f;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.05f;
        var trailMat = new Material(Shader.Find("Sprites/Default"));
        trail.material = trailMat;
        trail.startColor = new Color(1f, 0.5f, 0.1f, 1f);
        trail.endColor = new Color(1f, 0.2f, 0f, 0f);

        // Point Light
        var lightGO = new GameObject("BombLight");
        lightGO.transform.SetParent(bomb.transform, false);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.4f, 0.1f);
        light.intensity = 3f;
        light.range = 5f;

        currentBomb = bomb;
        lastDestroyedTime = Time.time;

        Debug.Log("[BombSpawner] 새 폭탄 생성!");
    }
}
