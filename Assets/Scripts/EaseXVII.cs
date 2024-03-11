using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class EaseXVII
{
    const MethodImplOptions inline = MethodImplOptions.AggressiveInlining;
    public enum Ease 
    { 
        Linear,
        InSine, OutSine, InOutSine,
        InQuad, OutQuad, InOutQuad,
        InCubic, OutCubic, InOutCubic,
        InQuart, OutQuart, InOutQuart,
        InQuint, OutQuint, InOutQuint,
        InExpo, OutExpo, InOutExpo,
        InBack, OutBack, InOutBack,
        InElastic, OutElastic, InOutElastic,
        InBounce, OutBounce, InOutBounce
    };

    public static float Evaluate(float x, Ease ease)
    {
        switch (ease)
        {
            case Ease.Linear:
                return x;

            case Ease.InSine:
                return InSine(x);
            case Ease.OutSine:
                return OutSine(x);
            case Ease.InOutSine:
                return InOutSine(x);
            case Ease.InQuad:

                return InQuad(x);
            case Ease.OutQuad:
                return OutQuad(x);  
            case Ease.InOutQuad:
                return InOutQuad(x);

            case Ease.InCubic:
                return InCubic(x);
            case Ease.OutCubic:
                return OutCubic(x);
            case Ease.InOutCubic:
                return InOutCubic(x);

            case Ease.InQuart:
                return InQuart(x);
            case Ease.OutQuart:
                return OutQuart(x);
            case Ease.InOutQuart:
                return InOutQuart(x);

            case Ease.InQuint:
                return InQuint(x);
            case Ease.OutQuint:
                return OutQuint(x);
            case Ease.InOutQuint:
                return InOutQuint(x);

            case Ease.InExpo:
                return InExpo(x);
            case Ease.OutExpo:
                return OutExpo(x);
            case Ease.InOutExpo:
                return InOutExpo(x);

            case Ease.InBack:
                return InBack(x);
            case Ease.OutBack:
                return OutBack(x);
            case Ease.InOutBack:
                return InOutBack(x);

            case Ease.InElastic:
                return InElastic(x);
            case Ease.OutElastic:
                return OutElastic(x);
            case Ease.InOutElastic:
                return InOutElastic(x);

            case Ease.InBounce:
                return InBounce(x);
            case Ease.OutBounce:
                return OutBounce(x);
            case Ease.InOutBounce:
                return InOutBounce(x);
        }
        return x;
    }


    [MethodImpl(inline)]
    public static float InSine(float x)
    {
        return 1 - math.cos((x * math.PI) / 2);
    }
    [MethodImpl(inline)]
    public static float OutSine(float x)
    {
        return math.sin((x * math.PI) / 2);
    }
    [MethodImpl(inline)]
    public static float InOutSine(float x)
    {
        return -(math.cos(math.PI * x) - 1) / 2;
    }

    [MethodImpl(inline)]
    public static float InQuad(float x)
    {
        return x * x;
    }
    [MethodImpl(inline)]
    public static float OutQuad(float x)
    {
        return 1 - (1 - x) * (1 - x);
    }
    [MethodImpl(inline)]
    public static float InOutQuad(float x)
    {
        return x < 0.5 ?
            2 * x * x :
            1 - (-2 * x + 2) * (-2 * x + 2) / 2;
    }
    [MethodImpl(inline)]
    public static float InCubic(float x)
    {
        return x * x * x;
    }
    [MethodImpl(inline)]
    public static float OutCubic(float x)
    {
        float oneMinX = 1 - x;
        return 1 - oneMinX * oneMinX * oneMinX;
    }
    [MethodImpl(inline)]
    public static float InOutCubic(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - math.pow(-2 * x + 2, 3) / 2;
    }
    [MethodImpl(inline)]
    public static float InQuart(float x)
    {
        return x * x * x * x;
    }
    [MethodImpl(inline)]
    public static float OutQuart(float x)
    {
        float oneMinX = 1 - x;
        return 1 - oneMinX * oneMinX * oneMinX * oneMinX;
    }
    [MethodImpl(inline)]
    public static float InOutQuart(float x)
    {
        return x < 0.5 ? 8 * x * x * x * x : 1 - math.pow(-2 * x + 2, 4) / 2;
    }
    [MethodImpl(inline)]

    public static float InQuint(float x)
    {
        return x * x * x * x * x;
    }
    [MethodImpl(inline)]
    public static float OutQuint(float x)
    {
        float oneMinX = 1 - x;
        return 1 - oneMinX * oneMinX * oneMinX * oneMinX * oneMinX;
    }
    [MethodImpl(inline)]
    public static float InOutQuint(float x)
    {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - math.pow(-2 * x + 2, 5) / 2;
    }
    [MethodImpl(inline)]

    public static float InExpo(float x)
    {
        return x == 0 ? 0 : math.pow(2, 10 * x - 10);
    }
    [MethodImpl(inline)]
    public static float OutExpo(float x)
    {
        return x == 1 ? 1 : 1 - math.pow(2, -10 * x);
    }
    [MethodImpl(inline)]
    public static float InOutExpo(float x)
    {
        return x == 0
          ? 0
          : x == 1
          ? 1
          : x < 0.5 ? math.pow(2, 20 * x - 10) / 2
          : (2 - math.pow(2, -20 * x + 10)) / 2;
    }
    [MethodImpl(inline)]

    public static float InBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;
        return c3 * x * x * x - c1 * x * x;
    }
    [MethodImpl(inline)]
    public static float OutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1;

        return 1 + c3 * math.pow(x - 1, 3) + c1 * math.pow(x - 1, 2);
    }
    [MethodImpl(inline)]
    public static float InOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c2 = c1 * 1.525f;

        return x < 0.5
          ? (math.pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
          : (math.pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
    }
    [MethodImpl(inline)]

    public static float InElastic(float x)
    {
        const float c4 = (2 * math.PI) / 3;
        return x == 0
          ? 0
          : x == 1
          ? 1
          : -math.pow(2, 10 * x - 10) * math.sin((x * 10 - 10.75f) * c4);
    }
    [MethodImpl(inline)]
    public static float OutElastic(float x)
    {
        const float c4 = (2 * math.PI) / 3;

        return x == 0
          ? 0
          : x == 1
          ? 1
          : math.pow(2, -10 * x) * math.sin((x * 10 - 0.75f) * c4) + 1;
    }
    [MethodImpl(inline)]
    public static float InOutElastic(float x)
    {
        const float c5 = (2 * math.PI) / 4.5f;
        return x == 0
          ? 0
          : x == 1
          ? 1
          : x < 0.5
          ? -(math.pow(2, 20 * x - 10) * math.sin((20 * x - 11.125f) * c5)) / 2
          : (math.pow(2, -20 * x + 10) * math.sin((20 * x - 11.125f) * c5)) / 2 + 1;
    }

    [MethodImpl(inline)]
    public static float InBounce(float x)
    {
        return 1 - OutBounce(1 - x);
    }
    [MethodImpl(inline)]
    public static float OutBounce(float x)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }

    [MethodImpl(inline)]
    public static float InOutBounce(float x)
    {
        return x < 0.5f
            ? (1 - OutBounce(1 - 2 * x)) / 2
            : (1 + OutBounce(2 * x - 1)) / 2;
    }
}
