using Unity.Mathematics;

namespace OFogo
{
    public class LineFireStroke : FireStroke
    {
        public LineFireStrokeContainer fireStroke;
        public bool isWorldSpace;
        public override FireStrokeContainer CreateFireStrokeContainer()
        {
            var strokeCopy = fireStroke;
            if(!isWorldSpace)
            {
                strokeCopy.posA = transform.TransformPoint(strokeCopy.posA);
                strokeCopy.posB = transform.TransformPoint(strokeCopy.posB);
            }

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