using OFogo;
using Unity.Mathematics;
using UnityEngine;

public class EmptyVectorField : VectorFieldGenerator
{
    public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
    {
        //
    }
}
