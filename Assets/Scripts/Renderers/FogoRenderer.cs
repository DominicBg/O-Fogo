using Unity.Collections;

namespace OFogo
{
    public abstract class FogoRenderer : AlphaRenderer
    {
        public abstract void Init(int particleCount);
        public abstract void Render(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings);
        public abstract void Dispose();
    }
}