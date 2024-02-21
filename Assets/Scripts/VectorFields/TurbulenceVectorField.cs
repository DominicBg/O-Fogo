using OFogo;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TurbulenceVectorField : SimpleVectorFieldGenerator
{
    [SerializeField] float scale;
    [SerializeField] float strength;
    [SerializeField] float2 offset;
    [SerializeField] float2 offsetOverTime;

    public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
    {
        for (int x = 0; x < vectorField.Size.x; x++)
        {
            for (int y = 0; y < vectorField.Size.y; y++)
            {
                float noiseAngle = noise.cnoise(new float2(x, y) * scale + offset + offsetOverTime * Time.fixedTime) * math.PI * 2f;
                math.sincos(noiseAngle, out float sin, out float cos);
                vectorField[x, y] = new float3(sin, cos, 0) * strength;
            }
        }
    }
}
