using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AutoMover : MonoBehaviour
{
    public float3 startPos = 0;
    public float radius = 2;
    public float rotationSpeed = 1;

    // Update is called once per frame
    void Update()
    {
        transform.position = startPos + math.mul(float3x3.RotateZ(math.radians(Time.time * rotationSpeed)), math.up()) * radius;
    }
}
