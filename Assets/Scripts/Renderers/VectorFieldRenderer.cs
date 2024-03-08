using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class VectorFieldRenderer : AlphaRenderer
    {
        public abstract void Init(in NativeGrid<float3> vectorField);
        public abstract void Render(in NativeGrid<float3> vectorField, in SimulationSettings settings);
        public abstract void Dispose();
    }
}