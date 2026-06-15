using UnityEngine;

public class CubeDamage : MonoBehaviour
{
    public float maxHp = 100f;
    float currentHp;

    Rigidbody rb;

    bool collapsed = false;

    void Start()
    {
        currentHp = maxHp;

        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void TakeDamage(float damage)
    {
        if (collapsed) return;

        currentHp -= damage;

        Debug.Log(
            gameObject.name +
            " HP : " +
            currentHp
        );

        if (currentHp <= 0)
        {
            Collapse();
        }
    }

    void Collapse()
    {
        collapsed = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        Debug.Log(gameObject.name + " ºØ±«");
    }
}