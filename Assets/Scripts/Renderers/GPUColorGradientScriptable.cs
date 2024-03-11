using System.Collections.Generic;
using UnityEngine;
namespace OFogo
{
    [System.Serializable]
    public struct GPUColorGradient
    {
        public List<Color> colors;
    }

    [CreateAssetMenu(fileName = "GPUColorGradient", menuName = "Gradient/GPUColorGradient", order = 1)]
    public class GPUColorGradientScriptable : ScriptableObject
    {
        public GPUColorGradient gpuGradient;
    }
}