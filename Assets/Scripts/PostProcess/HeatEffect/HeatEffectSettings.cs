using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("HeatEffect")]
public class HeatEffectSettings : VolumeComponent, IPostProcessComponent
{
    public ClampedFloatParameter blend = new ClampedFloatParameter(0.5f, 0.0f, 1.0f);
    public Vector2Parameter sinAmplitude = new Vector2Parameter(Vector2.one * 0.01f);
    public Vector2Parameter sinFrequency = new Vector2Parameter(Vector2.one);
    public Vector2Parameter waveEffect = new Vector2Parameter(Vector2.one);
    public FloatParameter verticalScroll = new FloatParameter(0.1f);
    
    public bool IsActive()
    {
        return blend.value > 0;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
