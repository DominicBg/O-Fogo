using Unity.Mathematics;
using UnityEngine;
namespace OFogo
{
    public class FixedDirectionVectorField : VectorFieldGenerator
    {
        [SerializeField] float3 force;

        protected override void OnUpdateVectorField(in SimulationData simData, ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            for (int x = 0; x < vectorField.Size.x; x++)
            {
                for (int y = 0; y < vectorField.Size.y; y++)
                {
                    vectorField[x, y] = force;
                }
            }
        }
    }
}