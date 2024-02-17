using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class VectorFieldGenerator : MonoBehaviour
    {
        [SerializeField] int2 vectorFieldSize;
        public virtual NativeGrid<float3> CreateVectorField(in Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            return new NativeGrid<float3>(vectorFieldSize, allocator);
        }
        public virtual void Init() { }
        public virtual void Dispose() { }
        public abstract void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds);
    }
}