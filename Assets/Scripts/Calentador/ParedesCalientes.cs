using Unity.Burst;
using Unity.Collections;

namespace OFogo
{
    [BurstCompile]
    public struct ParedesCalientes : ICalentadorJobParralel
    {
        public NativeArray<FireParticle> fireParticles;
        public SimulationSettings settings;

        public void Execute(int index)
        {
            FireParticle fireParticleParticle = fireParticles[index];
            WarmupParticle(ref fireParticleParticle);
            fireParticles[index] = fireParticleParticle;
        }

        public void WarmupParticle(ref FireParticle particle)
        {

        }
    }
}