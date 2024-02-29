using Unity.Mathematics;
using UnityEngine;
using static BezierFireStroke;
using static CircleFireStroke;
using static LineFireStroke;
using static PartialCircleFireStroke;

public abstract class FireStroke : MonoBehaviour
{
    public abstract FireStrokeContainer CreateFireStrokeContainer();
}

//This is a bit nasty, but this is supported for burst
public struct FireStrokeContainer
{
    public enum StrokeType { Circle, PartialCircle, Line, Bezier };
    public StrokeType strokeType;

    public CircleFireStrokeContainer circleFireStrokeContainer;
    public PartialCircleFireStrokeContainer partialCircleFireStrokeContainer;
    public LineFireStrokeContainer lineFireStrokeContainer;
    public BezierFireStrokeContainer bezierFireStrokeContainer;

    public float3 Evaluate(float t)
    {
        switch (strokeType)
        {
            case StrokeType.Circle: return circleFireStrokeContainer.Evaluate(t);
            case StrokeType.PartialCircle: return partialCircleFireStrokeContainer.Evaluate(t);
            case StrokeType.Line: return lineFireStrokeContainer.Evaluate(t);
            case StrokeType.Bezier: return bezierFireStrokeContainer.Evaluate(t);
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
        }
        return 0;
    }
}
public interface IFireStroke
{
    float3 Evaluate(float t);
    float GetLength();
}
