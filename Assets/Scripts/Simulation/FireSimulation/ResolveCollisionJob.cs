using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace OFogo
{
    [BurstCompile]
    public struct ResolveCollisionJob : IJob
    {
        public NativeArray<FireParticle> fireParticles;
        public NativeList<FireParticleCollision> fireParticleCollisionPair;
        public SimulationSettings settings;
        public NativeReference<Unity.Mathematics.Random> rngRef;

        public void Execute()
        {
            var rng = rngRef.Value;

            //resolve collision
            for (int i = 0; i < fireParticleCollisionPair.Length; i++)
            {
                FireParticleCollision pair = fireParticleCollisionPair[i];
                FireParticle particleA = fireParticles[pair.indexA];
                FireParticle particleB = fireParticles[pair.indexB];

                float dist;
                float3 dir;
                if (math.lengthsq(pair.distSq) <= math.FLT_MIN_NORMAL)
                {
                    dir = new float3(rng.NextFloat2Direction(), 0.0f);
                    dist = (particleA.radius + particleB.radius) * 0.1f;
                }
                else
                {
                    dist = math.sqrt(pair.distSq);
                    float3 diff = particleA.position - particleB.position;
                    dir = diff / dist;
                }

                float penetration = (particleA.radius + particleB.radius) - dist;

                float3 delta = 0.5f * dir * penetration;

                particleA.position += delta * settings.resolutionStepRatio;
                particleB.position -= delta * settings.resolutionStepRatio;

                particleA.velocity += delta * settings.colisionVelocityResolution;
                particleB.velocity -= delta * settings.colisionVelocityResolution;

                OFogoHelper.ApplyConstraintBounce(ref particleA, settings);
                OFogoHelper.ApplyConstraintBounce(ref particleB, settings);

                fireParticles[pair.indexA] = particleA;
                fireParticles[pair.indexB] = particleB;
            }

            rngRef.Value = rng;
        }
    }
}