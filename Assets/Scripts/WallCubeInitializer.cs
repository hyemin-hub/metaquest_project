using UnityEngine;
using System.Collections.Generic;

public class WallCubeInitializer : MonoBehaviour
{
    public Rigidbody[] cubes;

    void Awake()
    {
        List<Rigidbody> cubeList = new List<Rigidbody>();

        Rigidbody[] found = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in found)
        {
            if (rb.gameObject == gameObject) continue; // Wall 제외

            cubeList.Add(rb);

            // CubeDamage 자동 부착
            if (rb.GetComponent<CubeDamage>() == null)
            {
                rb.gameObject.AddComponent<CubeDamage>();
            }
        }

        cubes = cubeList.ToArray();
    }
}