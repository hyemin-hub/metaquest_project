using UnityEngine;

/// <summary>
/// 폭발 충격파 효과 — 구체가 빠르게 커지면서 투명해짐.
/// ExplosiveProjectile이 자동으로 추가/관리.
/// </summary>
public class ExplosionShockwave : MonoBehaviour
{
    public float maxScale = 6f;
    public float duration = 0.5f;

    private float elapsed;
    private Renderer rend;
    private Color baseColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) baseColor = rend.material.color;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / duration;
        if (t >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        float scale = Mathf.Lerp(0.5f, maxScale, t);
        transform.localScale = Vector3.one * scale;

        if (rend != null)
        {
            var c = baseColor;
            c.a = Mathf.Lerp(0.7f, 0f, t);
            rend.material.color = c;
        }
    }
}
