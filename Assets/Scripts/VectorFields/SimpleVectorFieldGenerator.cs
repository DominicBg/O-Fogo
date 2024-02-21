using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class SimpleVectorFieldGenerator : VectorFieldGenerator
    {
        [SerializeField] int2 vectorFieldSize;
        public override NativeGrid<float3> CreateVectorField(in Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            return new NativeGrid<float3>(vectorFieldSize, allocator);
        }
    }
}
