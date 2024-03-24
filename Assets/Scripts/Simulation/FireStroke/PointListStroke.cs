using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class PointListStroke : FireStroke
    {
        UnsafeList<float3> points;
        UnsafeList<float> cumulativeRatio;

        protected virtual void Start()
        {
            const int defaultCapacity = 128;
            points = new UnsafeList<float3>(defaultCapacity, Allocator.Persistent);
            cumulativeRatio = new UnsafeList<float>(defaultCapacity, Allocator.Persistent);
        }
        protected virtual void OnDestroy()
        {
            points.Dispose();
            cumulativeRatio.Dispose();
        }

        public void CopyPointsToStroke(in NativeList<float3> points)
        {
            this.points.Clear();
            for (int i = 0; i < points.Length; i++)
            {
                this.points.Add(points[i]);
            }
        }
        public void CopyPointsToStroke(in NativeCircularBuffer<float3> points)
        {
            this.points.Clear();
            for (int i = 0; i < points.Length; i++)
            {
                this.points.Add(points[i]);
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

        public float CalculateLength(UnsafeList<float3> points, ref UnsafeList<float> cumulativeRatio)
        {
            cumulativeRatio.Length = points.Length;
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
            public UnsafeList<float3> points;
            public UnsafeList<float> cumulativeRatio;
            public float length;

            public float3 Evaluate(float t)
            {
                for (int i = 1; i < points.Length; i++)
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