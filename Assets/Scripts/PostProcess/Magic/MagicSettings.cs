using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Magic")]
public class MagicSettings : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter alphaThreshold = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);
    public FloatParameter luminocityPower = new FloatParameter(1.0f);

    public bool IsActive()
    {
        return (alphaThreshold.value > 0.0f) && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
