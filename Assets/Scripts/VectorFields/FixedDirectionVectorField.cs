using Unity.Mathematics;
using UnityEngine;
namespace OFogo
{
    public class FixedDirectionVectorField : VectorFieldGenerator
    {
        [SerializeField] float3 force;

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
        {
            foreach(int2 pos in vectorField.GetIterator())
            {
                vectorField[pos] = force;
            }
        }
    }
}