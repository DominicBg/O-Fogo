using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class RadialVectorField : VectorFieldGenerator
    {
        [SerializeField] float force;
        [SerializeField] float angle;

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            float theta = math.radians(angle);
            math.sincos(theta, out float sin, out float cos);
            float2x2 rotationMatrix = new float2x2()
            {
                c0 = new float2(cos, sin),
                c1 = new float2(-sin, cos),
            };

            for (int x = 0; x < vectorField.Size.x; x++)
            {
                for (int y = 0; y < vectorField.Size.y; y++)
                {
                    int2 pos = new int2(x, y);
                    float2 dir = pos - (vectorField.Size / 2);
                    dir = math.mul(rotationMatrix, dir);
                    vectorField[pos] = math.normalizesafe(new float3(dir.xy, 0f)) * force;
                }
            }
        }
    }
}