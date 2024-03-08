using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static OFogo.OFogoSimulator;

namespace OFogo
{
    [BurstCompile]
    public struct ResolveTemperatureTransferJob : IJob
    {
        public NativeArray<FireParticle> fireParticles;
        public NativeList<FireParticleCollision> fireParticleCollisionPair;
        public SimulationSettings settings;
        public InternalSettings internalSettings;

        public void Execute()
        {
            for (int i = 0; i < fireParticleCollisionPair.Length; i++)
            {
                FireParticleCollision pair = fireParticleCollisionPair[i];
                FireParticle particleA = fireParticles[pair.indexA];
                FireParticle particleB = fireParticles[pair.indexB];

                float tempA = particleA.temperature;
                float tempB = particleB.temperature;
                float t = internalSettings.heatTransferPercent;
                particleA.temperature = math.lerp(tempA, tempB, t);
                particleB.temperature = math.lerp(tempB, tempA, t);

                fireParticles[pair.indexA] = particleA;
                fireParticles[pair.indexB] = particleB;
            }
        }
    }
}
