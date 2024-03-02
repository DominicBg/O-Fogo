using Unity.Mathematics;

namespace OFogo
{
    public class CircleFireStroke : FireStroke
    {
        public CircleFireStrokeContainer fireStroke = new CircleFireStrokeContainer()
        {
            normal = math.forward(),
            radius = 1,
        };

        public override FireStrokeContainer CreateFireStrokeContainer()
        {
            var strokeCopy = fireStroke;
            strokeCopy.position += (float3)transform.position;

            return new FireStrokeContainer()
            {
                strokeType = FireStrokeContainer.StrokeType.Circle,
                circleFireStrokeContainer = strokeCopy
            };
        }

        [System.Serializable]
        public struct CircleFireStrokeContainer : IFireStroke
        {
            public float3 position;
            public float3 normal;
            public float radius;

            public float3 Evaluate(float t)
            {
                float a = t * math.PI * 2;
                math.sincos(a, out float sin, out float cos);

                float3 up = math.dot(normal, math.up()) < 0.999 ? math.up() : math.forward();
                float3 right = math.cross(up, normal);

                return position + (up * cos - right * sin) * radius;
            }

            public float GetLength()
            {
                return 2 * math.PI * radius;
            }
        }
    }
}