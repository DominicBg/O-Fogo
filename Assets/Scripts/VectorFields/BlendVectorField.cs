using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class BlendVectorField : VectorFieldGenerator
    {
        [Range(0, 1)]
        public float ratio;
        public VectorFieldGenerator vectorFieldGeneratorA;
        public VectorFieldGenerator vectorFieldGeneratorB;

        NativeGrid<float3> vectorField1;
        NativeGrid<float3> vectorField2;

        public override void OnInit(in SimulationSettings settings)
        {
            vectorField1 = CreateVectorField(settings, Allocator.Persistent);
            vectorField2 = CreateVectorField(settings, Allocator.Persistent);
        }

        protected override void OnUpdateVectorField(in SimulationData simData, ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            vectorFieldGeneratorA.UpdateVectorField(simData, ref vectorField1, in settings);
            vectorFieldGeneratorB.UpdateVectorField(simData, ref vectorField2, in settings);

            for (int x = 0; x < vectorField1.Size.x; x++)
            {
                for (int y = 0; y < vectorField1.Size.y; y++)
                {
                    vectorField[x, y] = math.lerp(vectorField1[x, y], vectorField2[x, y], ratio);
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