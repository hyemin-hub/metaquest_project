using UnityEngine;

public class BallDamage : MonoBehaviour
{
    public float damage = 5f;

    void OnCollisionEnter(Collision collision)
    {
        TowerHealth tower =
            collision.gameObject.GetComponentInParent<TowerHealth>();

        Debug.Log("Hit : " + collision.gameObject.name + ", Tower : " + (tower != null));

        if (tower != null)
        {
            tower.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}