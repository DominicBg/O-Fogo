using Unity.Mathematics;

namespace OFogo
{
    public class LineFireStroke : FireStroke
    {
        public LineFireStrokeContainer fireStroke;
        public override FireStrokeContainer CreateFireStrokeContainer()
        {
            var strokeCopy = fireStroke;
            strokeCopy.posA += (float3)transform.position;
            strokeCopy.posB += (float3)transform.position;

            return new FireStrokeContainer()
            {
                strokeType = FireStrokeContainer.StrokeType.Line,
                lineFireStrokeContainer = strokeCopy
            };
        }

        [System.Serializable]
        public struct LineFireStrokeContainer : IFireStroke
        {
            public float3 posA;
            public float3 posB;

            public float3 Evaluate(float t)
            {
                return math.lerp(posA, posB, t);
            }

            public float GetLength()
            {
                return math.distance(posA, posB);
            }
        }
    }
}