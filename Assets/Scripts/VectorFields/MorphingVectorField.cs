using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class MorphingVectorField : VectorFieldGenerator
    {
        [SerializeField] SerializedVectorField[] fields;
        [SerializeField] float morphSpeed = 0.5f;
        [SerializeField] float timeOffset;
        [SerializeField] float forceMultiplier = 1;
        NativeGrid<float3>[] vectorFields;
        int2 size;
        public override void Init()
        {
            base.Init();
            vectorFields = new NativeGrid<float3>[fields.Length];

            if (fields.Length == 0)
            {
                Debug.LogError(nameof(fields) + " can't be null.");
                return;
            }

            size = fields[0].size;
            for (int i = 0; i < fields.Length; i++)
            {
                vectorFields[i] = fields[i].Deserialize(Allocator.Persistent);
                if (math.any(vectorFields[i].Size != size))
                {
                    Debug.LogError("Vector fields aren't the same size.");
                    return;
                }
            }
        }

        public override NativeGrid<float3> CreateVectorField(in Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            return new NativeGrid<float3>(size, allocator);
        }

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
        {
            float time = (timeOffset + Time.time) * morphSpeed;
            int currentIndex = (int)math.floor(time) % vectorFields.Length;
            int nextIndex = (currentIndex + 1) % vectorFields.Length;
            float t = math.frac(time);
            t = EaseInOutCubic(t);

            NativeGrid<float3> v1 = vectorFields[currentIndex];
            NativeGrid<float3> v2 = vectorFields[nextIndex];

            for (int x = 0; x < v1.Size.x; x++)
            {
                for (int y = 0; y < v1.Size.x; y++)
                {
                    vectorField[x, y] = math.lerp(v1[x, y], v2[x, y], t) * forceMultiplier;
                }
            }
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