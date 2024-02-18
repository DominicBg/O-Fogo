using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Magic")]
public class MagicSettings : VolumeComponent, IPostProcessComponent
{
    //public ClampedFloatParameter alphaThreshold = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);

    public ClampedFloatParameter minRemap = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public ClampedFloatParameter maxRemap = new ClampedFloatParameter(1.0f, 0.0f, 1.0f);

    public FloatParameter luminocityPower = new FloatParameter(0.25f);

    public FloatParameter quantize = new FloatParameter(2000.0f);
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
        return active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }

}
