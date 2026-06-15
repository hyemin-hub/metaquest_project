using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    public float hp = 100f;

    bool collapsed = false;

    public float explosionForce = 300f;
    public float explosionRadius = 5f;
    public float upwardModifier = 0.5f;
    public float torque = 50f;

    public void TakeDamage(float damage)
    {
        if (collapsed) return;

        hp -= damage;

        Debug.Log("Tower HP = " + hp);

        if (hp <= 0)
        {
            Collapse();
        }
    }

    void Collapse()
    {
        collapsed = true;

        // 🔥 자동 수집 (CubeManager랑 동일 방식)
        Rigidbody[] cubes = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in cubes)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            rb.AddTorque(Random.insideUnitSphere * torque, ForceMode.Impulse);
        }

        transform.DetachChildren();
    }
}