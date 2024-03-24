using Unity.Mathematics;
using UnityEngine;

public class SerpenteDiFuoco : MonoBehaviour
{
    public float3 startPos = 0;
    public float radius = 2;
    public float rotationSpeed = 1;
    public float heightSinAmplitude = 1;
    public float heightSinFrequency = 1;
    public float heightSinOffset = 0;

    void Update()
    {
        transform.position = CalculatePos(heightSinAmplitude, heightSinFrequency, heightSinOffset, radius, rotationSpeed, Time.time);
    }

    static float3 CalculatePos(float a, float f, float p, float r, float s, float t)
    {
        float3 y = math.up() * a * math.sin(t * f * math.PI * 2f + p);
        float3 xz = math.mul(float3x3.RotateZ(math.radians(s * t)), math.up()) * r;
        return y + xz;
    }
}
