using Unity.Collections;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace OFogo
{
    public class MorphingVectorField : VectorFieldGenerator
    {
        [SerializeField] VectorFieldGenerator[] fields;
        [SerializeField] float morphSpeed = 0.5f;
        [SerializeField] float timeOffset;
        [SerializeField] float forceMultiplier = 1;

        [SerializeField] MagicController hackParceQueJmenCriss;

        NativeGrid<float3>[] vectorFields;

        public override void OnInit(in SimulationSettings settings)
        {
            vectorFields = new NativeGrid<float3>[fields.Length];

            if (fields.Length == 0)
            {
                Debug.LogError(nameof(fields) + " can't be null.");
            }

            for (int i = 0; i < fields.Length; i++)
            {
                vectorFields[i] = CreateVectorField(in settings, Allocator.Persistent);
            }
        }


        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            float time = (timeOffset + Time.time) * morphSpeed;
            int currentIndex = (int)math.floor(time) % vectorFields.Length;
            int nextIndex = (currentIndex + 1) % vectorFields.Length;
            float t = math.frac(time);
            t = EaseInOutCubic(t);

            NativeGrid<float3> v1 = vectorFields[currentIndex];
            NativeGrid<float3> v2 = vectorFields[nextIndex];

            fields[currentIndex].UpdateVectorField(ref v1, in settings);
            fields[nextIndex].UpdateVectorField(ref v2, in settings);

            for (int x = 0; x < v1.Size.x; x++)
            {
                for (int y = 0; y < v1.Size.y; y++)
                {
                    vectorField[x, y] = math.lerp(v1[x, y], v2[x, y], t) * forceMultiplier;
                }
            }

            hackParceQueJmenCriss?.LerpGradient(currentIndex, nextIndex, t);
        }

        public override void Dispose()
        {
            base.Dispose();
            for (int i = 0; i < vectorFields.Length; i++)
            {
                vectorFields[i].Dispose();
            }
        }

        float EaseInOutCubic(float x)
        {
            return x < 0.5 ? 4 * x * x * x : 1 - math.pow(-2 * x + 2, 3) / 2;
        }
    }
}