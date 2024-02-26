using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class VectorFieldGenerator : MonoBehaviour
    {
        public virtual NativeGrid<float3> CreateVectorField(int2 size, in Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            return new NativeGrid<float3>(size, allocator);
        }
        public abstract void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds);
        public virtual void Dispose() { }
    }
}