using Unity.Burst;
using Unity.Collections;

namespace OFogo
{
    [BurstCompile]
    public struct RuidoDeFuego : ICalentador
    {
        public NativeArray<FireParticle> fireParticles;
        public SimulationSettings settings;

        public void Execute(int index)
        {
            FireParticle fireParticleParticle = fireParticles[index];
            HeatParticle(ref fireParticleParticle);
            fireParticles[index] = fireParticleParticle;
        }

        public void HeatParticle(ref FireParticle particle)
        {
            //todo
        }
    }
}