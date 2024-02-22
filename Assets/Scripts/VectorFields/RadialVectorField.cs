using Unity.Mathematics;
using UnityEngine;
namespace OFogo
{
    public class RadialVectorField : VectorFieldGenerator
    {
        [SerializeField] float force;

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
        {
            foreach(int2 pos in vectorField.GetIterator())
            {
                float2 dir = pos - (vectorField.Size / 2);
                vectorField[pos] = math.normalize(new float3(dir.xy, 0f)) * force;
            }
        }
    }
}