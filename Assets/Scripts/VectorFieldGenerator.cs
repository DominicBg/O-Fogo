using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class VectorFieldGenerator : MonoBehaviour
    {
        public abstract NativeGrid<float3> CreateVectorField(in Bounds bounds, Allocator allocator = Allocator.Persistent);
        public abstract void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds);
    }
}