using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class EmptyVectorField : VectorFieldGenerator
    {
        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            //
        }
    }
}