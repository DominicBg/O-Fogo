using Unity.Mathematics;
using UnityEngine;
using static OFogo.BezierFireStroke;
using static OFogo.CircleFireStroke;
using static OFogo.LineFireStroke;
using static OFogo.PartialCircleFireStroke;
using static OFogo.PointListStroke;

namespace OFogo
{
    public abstract class FireStroke : MonoBehaviour
    {
        public abstract FireStrokeContainer CreateFireStrokeContainer();
    }

    //This is a bit nasty, but this is supported for burst
    public struct FireStrokeContainer
    {
        public enum StrokeType { Circle, PartialCircle, Line, Bezier, Trail };
        public StrokeType strokeType;

        public CircleFireStrokeContainer circleFireStrokeContainer;
        public PartialCircleFireStrokeContainer partialCircleFireStrokeContainer;
        public LineFireStrokeContainer lineFireStrokeContainer;
        public BezierFireStrokeContainer bezierFireStrokeContainer;
        public TrailFireStrokeContainer trailFireStrokeContainer;
        public bool useRatioAsHeat;

        public float3 Evaluate(float t)
        {
            switch (strokeType)
            {
                case StrokeType.Circle: return circleFireStrokeContainer.Evaluate(t);
                case StrokeType.PartialCircle: return partialCircleFireStrokeContainer.Evaluate(t);
                case StrokeType.Line: return lineFireStrokeContainer.Evaluate(t);
                case StrokeType.Bezier: return bezierFireStrokeContainer.Evaluate(t);
                case StrokeType.Trail: return trailFireStrokeContainer.Evaluate(t);
            }
            return 0;
        }
        public float GetLength()
        {
            switch (strokeType)
            {
                case StrokeType.Circle: return circleFireStrokeContainer.GetLength();
                case StrokeType.PartialCircle: return partialCircleFireStrokeContainer.GetLength();
                case StrokeType.Line: return lineFireStrokeContainer.GetLength();
                case StrokeType.Bezier: return bezierFireStrokeContainer.GetLength();
                case StrokeType.Trail: return trailFireStrokeContainer.GetLength();
            }
            return 0;
        }
    }
    public interface IFireStroke
    {
        float3 Evaluate(float t);
        float GetLength();
    }
}