using Unity.Mathematics;

namespace OFogo
{
    public abstract class VectorFieldRenderer : AlphaRenderer
    {
        public void Render(in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            OnRender(vectorField, settings);
            renderedLastFrame = true;
        }

        public abstract void Init(in NativeGrid<float3> vectorField);
        protected abstract void OnRender(in NativeGrid<float3> vectorField, in SimulationSettings settings);
        public abstract void OnStopRendering(in NativeGrid<float3> vectorField, in SimulationSettings settings);
        public abstract void Dispose();
    }
}