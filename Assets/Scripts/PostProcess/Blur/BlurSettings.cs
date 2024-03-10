using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Blur")]
public class BlurSettings : VolumeComponent, IPostProcessComponent
{
    public IntParameter gridCellCount = new IntParameter(5);
    public FloatParameter wideness = new FloatParameter(0.1f);
    public FloatParameter spread = new FloatParameter(1);

    public bool IsActive()
    {
        return (gridCellCount.value > 0) && active;
    }

    public bool IsTileCompatible()
    {
        return false;
    }
}
