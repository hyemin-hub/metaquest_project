using UnityEngine;

/// <summary>
/// 폭탄 투사체. 충돌 시 주변 블록에 폭발력을 부여하여 사방으로 날려버림.
/// </summary>
public class ExplosiveProjectile : Projectile
{
    [Header("폭발 설정")]
    [Tooltip("폭발 영향 범위 (m)")]
    public float explosionRadius = 3f;

    [Tooltip("주변 Rigidbody에 가할 폭발력")]
    public float explosionForce = 700f;

    [Tooltip("위쪽으로 살짝 띄우는 힘")]
    public float upwardModifier = 1f;

    [Header("이펙트")]
    [Tooltip("터질 때 생성할 불꽃/연기 파티클 프리팹")]
    public GameObject explosionVFX;

    [Tooltip("폭발음")]
    public AudioClip explosionSFX;

    protected override void Awake()
    {
        base.Awake();
        destroyOnHit = true;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (hasCollided) return;
        hasCollided = true;

        Vector3 explosionPos = transform.position;
        Collider[] hits = Physics.OverlapSphere(explosionPos, explosionRadius);

        int hitBlocks = 0;
        // 부모/매니저 중복 데미지 방지
        var towerSet = new System.Collections.Generic.HashSet<TowerHealth>();
        var mgrSet = new System.Collections.Generic.HashSet<CubeManager>();

        foreach (Collider hit in hits)
        {
            // 1. CubeDamage (개별 큐브)
            var cube = hit.GetComponent<CubeDamage>();
            if (cube != null)
            {
                cube.TakeDamage(damage);
                hitBlocks++;
            }

            // 2. TowerHealth (부모 Wall, 중복 방지)
            var tower = hit.GetComponentInParent<TowerHealth>();
            if (tower != null && towerSet.Add(tower))
            {
                tower.TakeDamage(damage);
            }

            // 3. CubeManager (부모 Wall, 중복 방지)
            var mgr = hit.GetComponentInParent<CubeManager>();
            if (mgr != null && mgrSet.Add(mgr))
            {
                mgr.TakeDamage(damage);
            }

            // 4. SimpleHealthBlock (시연용)
            var block = hit.GetComponent<SimpleHealthBlock>();
            if (block != null)
            {
                block.TakeDamage(damage);
                hitBlocks++;
            }

            // 5. 폭발력 부여
            Rigidbody hitRb = hit.GetComponent<Rigidbody>();
            if (hitRb != null && !hitRb.isKinematic)
            {
                hitRb.AddExplosionForce(explosionForce, explosionPos, explosionRadius, upwardModifier, ForceMode.Impulse);
            }
        }

        // VFX
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, explosionPos, Quaternion.identity);
        }
        else
        {
            // 코드로 자동 폭발 효과 (파티클 + 빛)
            CreateExplosionEffect(explosionPos);
        }

        if (explosionSFX != null) AudioSource.PlayClipAtPoint(explosionSFX, explosionPos);

        Debug.Log($"[Bomb] 폭발! 영향 오브젝트: {hits.Length}개, 부순 블록: {hitBlocks}개");
        Destroy(gameObject);
    }

    void CreateExplosionEffect(Vector3 pos)
    {
        // 1) 폭발 빛 (잠깐 강하게 켰다가 꺼짐)
        var lightGO = new GameObject("Explosion_Light");
        lightGO.transform.position = pos;
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.5f, 0.1f);
        light.intensity = 25f;
        light.range = 12f;
        Destroy(lightGO, 0.6f);

        // 2) 파티클 시스템 (불꽃)
        var psGO = new GameObject("Explosion_Particles");
        psGO.transform.position = pos;
        var ps = psGO.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 8f;
        main.startSize = 0.3f;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.8f, 0.1f),
            new Color(1f, 0.3f, 0.05f)
        );
        main.gravityModifier = 0.5f;
        main.maxParticles = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 100));

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 1f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 0.4f),
                new GradientColorKey(new Color(0.3f, 0.1f, 0.05f), 1f)
            },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = grad;

        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.3f);
        sizeCurve.AddKey(0.3f, 1f);
        sizeCurve.AddKey(1f, 0.2f);
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // 파티클 매테리얼
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Sprites/Default"));
        }

        Destroy(psGO, 2f);

        // 3) 충격파 sphere (살짝 커졌다 사라짐)
        var shockwave = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(shockwave.GetComponent<Collider>());
        shockwave.name = "Explosion_Shockwave";
        shockwave.transform.position = pos;
        shockwave.transform.localScale = Vector3.one * 0.5f;
        var sRend = shockwave.GetComponent<Renderer>();
        if (sRend != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            if (mat.shader == null) mat = new Material(Shader.Find("Unlit/Color"));
            mat.color = new Color(1f, 0.7f, 0.3f, 0.6f);
            sRend.material = mat;
        }
        shockwave.AddComponent<ExplosionShockwave>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
