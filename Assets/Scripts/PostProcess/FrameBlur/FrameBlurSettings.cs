using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Frame Blur 2")]
public class FrameBlurSettings : VolumeComponent, IPostProcessComponent
{
    public FloatParameter blend = new ClampedFloatParameter(.5f, 0f, 1f);

    public bool IsActive()
    {
        return blend.overrideState && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
