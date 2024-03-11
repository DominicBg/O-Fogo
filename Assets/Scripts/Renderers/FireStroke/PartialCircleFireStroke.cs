using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class PartialCircleFireStroke : FireStroke
    {
        public PartialCircleFireStrokeContainer fireStroke = new PartialCircleFireStrokeContainer()
        {
            normal = math.forward(),
            radius = 1,
            minRange = 0f,
            maxRange = 0.75f,
            rotationOffset = 0,
        };

        public override FireStrokeContainer CreateFireStrokeContainer()
        {
            var strokeCopy = fireStroke;
            strokeCopy.position += (float3)transform.position;
            strokeCopy.radius *= transform.lossyScale.x;

            return new FireStrokeContainer()
            {
                strokeType = FireStrokeContainer.StrokeType.PartialCircle,
                partialCircleFireStrokeContainer = strokeCopy
            };
        }

        [System.Serializable]
        public struct PartialCircleFireStrokeContainer : IFireStroke
        {
            public float3 position;
            public float3 normal;
            public float radius;

            [Range(0, 1)]
            public float rotationOffset;
            [Range(0, 1)]
            public float minRange;
            [Range(0, 1)]
            public float maxRange;

            public float3 Evaluate(float t)
            {
                t = math.remap(0, 1, minRange, maxRange, t);
                t += rotationOffset;

                float a = t * math.PI * 2;
                math.sincos(a, out float sin, out float cos);

                float3 up = math.dot(normal, math.up()) < 0.999 ? math.up() : math.forward();
                float3 right = math.cross(up, normal);

                return position + (up * cos - right * sin) * radius;
            }

            public float GetLength()
            {
                return 2 * math.PI * radius * (maxRange - minRange);
            }
        }
    }
}