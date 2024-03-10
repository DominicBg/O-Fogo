using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class BlendVectorField : VectorFieldGenerator
    {
        [Range(0, 1)]
        public float t;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator1;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator2;

        NativeGrid<float3> vectorField1;
        NativeGrid<float3> vectorField2;

        public override void OnInit(in SimulationSettings settings)
        {
            vectorField1 = CreateVectorField(settings, Allocator.Persistent);
            vectorField2 = CreateVectorField(settings, Allocator.Persistent);
        }

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            vectorFieldGenerator1.UpdateVectorField(ref vectorField1, in settings);
            vectorFieldGenerator2.UpdateVectorField(ref vectorField2, in settings);

            for (int x = 0; x < vectorField1.Size.x; x++)
            {
                for (int y = 0; y < vectorField1.Size.y; y++)
                {
                    vectorField[x, y] = math.lerp(vectorField1[x, y], vectorField2[x, y], t);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            vectorField1.Dispose();
            vectorField2.Dispose();
        }
    }
}