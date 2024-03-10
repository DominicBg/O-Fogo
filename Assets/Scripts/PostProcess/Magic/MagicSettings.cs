using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Magic")]
public class MagicSettings : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter minRemap = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public ClampedFloatParameter maxRemap = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);

    public FloatParameter luminocityPower = new FloatParameter(0.25f);

    public ClampedFloatParameter quantize = new ClampedFloatParameter(0.01f, 0.0f, 1.0f);
    public BoolParameter useDither = new BoolParameter(false);
    public BoolParameter diamondize = new BoolParameter(false);

    [Header("Colors")]
    public IntParameter colorCount = new IntParameter(3);
    public ColorParameter col0 = new ColorParameter(Color.white);
    public ColorParameter col1 = new ColorParameter(Color.gray);
    public ColorParameter col2 = new ColorParameter(Color.black);
    public ColorParameter col3 = new ColorParameter(Color.white);
    public ColorParameter col4 = new ColorParameter(Color.white);
    public ColorParameter col5 = new ColorParameter(Color.white);
    public ColorParameter col6 = new ColorParameter(Color.white);
    public ColorParameter col7 = new ColorParameter(Color.white);
    public ColorParameter col8 = new ColorParameter(Color.white);
    public ColorParameter col9 = new ColorParameter(Color.white);

    public bool IsActive()
    {
        return colorCount.value > 0;
    }

    public bool IsTileCompatible()
    {
        return false;
    }

    public void SetGradientIntoList(ref List<Color> gradient)
    {
        //lol, I guess its possible with reflection but didnt want to do GC
        gradient.Clear();

        int count = colorCount.value;
        if (count > 0) gradient.Add(col0.value);
        if (count > 1) gradient.Add(col1.value);
        if (count > 2) gradient.Add(col2.value);
        if (count > 3) gradient.Add(col3.value);
        if (count > 4) gradient.Add(col4.value);
        if (count > 5) gradient.Add(col5.value);
        if (count > 6) gradient.Add(col6.value);
        if (count > 7) gradient.Add(col7.value);
        if (count > 8) gradient.Add(col8.value);
        if (count > 9) gradient.Add(col9.value);
    }
    public void SetGradient(List<Color> gradient)
    {
        int count = gradient.Count;
        if (count > 0) col0.value = gradient[0];
        if (count > 1) col1.value = gradient[1];
        if (count > 2) col2.value = gradient[2];
        if (count > 3) col3.value = gradient[3];
        if (count > 4) col4.value = gradient[4];
        if (count > 5) col5.value = gradient[5];
        if (count > 6) col6.value = gradient[6];
        if (count > 7) col7.value = gradient[7];
        if (count > 8) col8.value = gradient[8];
        if (count > 9) col9.value = gradient[9];
    }
}
