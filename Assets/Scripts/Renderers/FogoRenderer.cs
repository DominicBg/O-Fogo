using Unity.Collections;
using Unity.Mathematics;

namespace OFogo
{
    public abstract class FogoRenderer : AlphaRenderer
    {
        public void Render(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings)
        {
            OnRender(fireParticles, settings);
            renderedLastFrame = true;
        }

        public abstract void Init(int particleCount);
        protected abstract void OnRender(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings);
        public abstract void OnStopRendering(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings);
        public abstract void Dispose();
    }
}