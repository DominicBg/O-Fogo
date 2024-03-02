using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class BlendVectorField : VectorFieldGenerator
    {
        [Range(0, 1)]
        public float t;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator1;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator2;

        NativeGrid<float3> vectorField1;
        NativeGrid<float3> vectorField2;

        public override NativeGrid<float3> CreateVectorField(int2 size, in Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            vectorField1 = vectorFieldGenerator1.CreateVectorField(size, in bounds, allocator);
            vectorField2 = vectorFieldGenerator2.CreateVectorField(size, in bounds, allocator);

            return vectorField1;
        }

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
        {
            vectorFieldGenerator1.UpdateVectorField(ref vectorField1, in bounds);
            vectorFieldGenerator2.UpdateVectorField(ref vectorField2, in bounds);

            for (int x = 0; x < vectorField1.Size.x; x++)
            {
                for (int y = 0; y < vectorField1.Size.y; y++)
                {
                    vectorField[x, y] = math.lerp(vectorField1[x, y], vectorField2[x, y], t);
                }
            }
        }
    }
}