using UnityEngine;
using UnityEngine.InputSystem;

// FirePoint

public class BallShooter : MonoBehaviour
{
    public GameObject ballPrefab;
    public Transform firePoint;
    public float shootForce = 700f;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject ball = Instantiate(
            ballPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        rb.AddForce(
            firePoint.forward * shootForce
        );
    }
}