using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    public float hp = 100f;

    public Rigidbody[] cubes;

    bool collapsed = false;

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

        transform.DetachChildren();

        foreach (Rigidbody rb in cubes)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddExplosionForce(300f, transform.position, 5f);
            rb.AddTorque(Random.insideUnitSphere * 50f, ForceMode.Impulse);
        }
    }
}