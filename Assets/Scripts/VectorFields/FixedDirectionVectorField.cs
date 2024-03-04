using Unity.Mathematics;
using UnityEngine;
namespace OFogo
{
    public class FixedDirectionVectorField : VectorFieldGenerator
    {
        [SerializeField] float3 force;

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in SimulationSettings settings)
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