using Unity.Mathematics;

namespace OFogo
{
    public class BezierFireStroke : FireStroke
    {
        public BezierFireStrokeContainer fireStroke;

        public override FireStrokeContainer CreateFireStrokeContainer()
        {
            var strokeCopy = fireStroke;
            strokeCopy.posA += (float3)transform.position;
            strokeCopy.posB += (float3)transform.position;
            strokeCopy.posC += (float3)transform.position;

            return new FireStrokeContainer()
            {
                strokeType = FireStrokeContainer.StrokeType.Bezier,
                bezierFireStrokeContainer = strokeCopy
            };
        }

        [System.Serializable]
        public struct BezierFireStrokeContainer : IFireStroke
        {
            public float3 posA;
            public float3 posB;
            public float3 posC;
            public float3 Evaluate(float t)
            {
                return math.lerp(math.lerp(posA, posB, t), math.lerp(posB, posC, t), t);
            }

            public float GetLength()
            {
                //fuck je sais pas lol
                return math.distance(posA, posB) + math.distance(posB, posC);
            }
        }
    }
}