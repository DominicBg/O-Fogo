using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class TrailStroke : FireStroke
    {
        NativeCircularBuffer<float3> points;
        UnsafeList<float> cumulativeRatio;
        [SerializeField] float samplePointsInterval = 0.2f;
        [SerializeField] int maxPointCount = 100;

        float currentSamplePointsDuration = 0;

        private void Start()
        {
            points = new NativeCircularBuffer<float3>(maxPointCount, Allocator.Persistent);
            cumulativeRatio = new UnsafeList<float>(maxPointCount, Allocator.Persistent);
            cumulativeRatio.Length = maxPointCount;
        }
        private void OnDestroy()
        {
            points.Dispose();
            cumulativeRatio.Dispose();
        }

        private void Update()
        {
            currentSamplePointsDuration += Time.deltaTime;
            if (currentSamplePointsDuration > samplePointsInterval)
            {
                currentSamplePointsDuration -= samplePointsInterval;

                if(currentSamplePointsDuration > samplePointsInterval)
                {
                    currentSamplePointsDuration = 0;
                }
                points.Add(transform.position);
            }
            DebugDraw();
        }

        void DebugDraw()
        {
            for (int i = 1; i < points.Length; i++)
            {
                Debug.DrawLine(points[i - 1], points[i]);
            }
        }

        public override FireStrokeContainer CreateFireStrokeContainer()
        {
            float lengthSum = CalculateLength(points, ref cumulativeRatio);
    
            var container = new FireStrokeContainer()
            {
                trailFireStrokeContainer = new TrailFireStrokeContainer()
                {
                    points = points,
                    cumulativeRatio = cumulativeRatio,
                    length = lengthSum
                },
                strokeType = FireStrokeContainer.StrokeType.Trail,
                useRatioAsHeat = true
            };

            return container;
        }

        public float CalculateLength(NativeCircularBuffer<float3> points, ref UnsafeList<float> cumulativeRatio)
        {
            cumulativeRatio[0] = 0;

            float sum = 0;
            for (int i = 1; i < points.Length; i++)
            {
                sum += math.distance(points[i - 1], points[i]);
                cumulativeRatio[i] = sum;
            }

            for (int i = 0; i < points.Length; i++)
            {
                cumulativeRatio[i] /= sum;
            }

            return sum;
        }

        [System.Serializable]
        public struct TrailFireStrokeContainer : IFireStroke
        {
            public NativeCircularBuffer<float3> points;
            public UnsafeList<float> cumulativeRatio;
            public float length;

            public float3 Evaluate(float t)
            {
                for (int i = 1; i < cumulativeRatio.Length; i++)
                {
                    float min = cumulativeRatio[i - 1];
                    float max = cumulativeRatio[i];
                    if (t > min && t <= max)
                    {
                        float ratio = math.unlerp(min, max, t);
                        return math.lerp(points[i - 1], points[i], ratio);
                    }
                }
                return points[0];
            }

            public float GetLength()
            {
                return length;
            }
        }
    }
}