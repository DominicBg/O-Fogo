using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class EmptyVectorField : VectorFieldGenerator
    {
        protected override void OnUpdateVectorField(in SimulationData simData, ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            //
        }
    }
}