using OFogo;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class NoVectorField : VectorFieldGenerator
{
    public override NativeGrid<float3> CreateVectorField(in Bounds bounds, Allocator allocator = Allocator.Persistent)
    {
        return new NativeGrid<float3>(1, allocator);
    }

    public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
    {
        //
    }
}
