using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    [BurstCompile]
    public class OFogoSimulator : MonoBehaviour, IFireParticleSimulator
    {
        public bool CanResolveCollision() => true;
        public bool IsHandlingParticleHeating() => false;
        public bool NeedsVectorField() => true;

        public uint seed = 43243215;
        public bool parallelCollision;

        NativeList<FireParticleCollision> fireParticleCollisionPair;
        Unity.Mathematics.Random rng;

        public void Init(in SimulationSettings settings)
        {
            rng = Unity.Mathematics.Random.CreateFromIndex(seed);
            fireParticleCollisionPair = new NativeList<FireParticleCollision>(GetMaxCollisionCount(settings.particleCount), Allocator.Persistent);
        }

        private int GetMaxCollisionCount(int particleCount)
        {
            // each particle can collid with every other particle n^2
            // if i hit j, we skip j hit i
            // n(n+1)/2 
            // we remove self particle hit
            return (particleCount * (particleCount - 1)) / 2;
        }

        public void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            new UpdateSimulationJob()
            {
                simulationData = simulationData,
                fireParticles = fireParticles,
                settings = settings,
                vectorField = vectorField
            }.RunParralel(fireParticles.Length);
        }

        public void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            fireParticleCollisionPair.Clear();
            if (parallelCollision)
            {
                new FindCollisionPairParallelJob()
                {
                    fireParticles = fireParticles,
                    fireParticleCollisionPair = fireParticleCollisionPair.AsParallelWriter(),
                    nativeHashingGrid = nativeHashingGrid,
                    settings = settings
                }.RunParralel(fireParticles.Length);
            }
            else
            {
                new FindCollisionPairJob()
                {
                    fireParticles = fireParticles,
                    fireParticleCollisionPair = fireParticleCollisionPair,
                    nativeHashingGrid = nativeHashingGrid,
                    settings = settings
                }.Run();
            }

            var rngRef = new NativeReference<Unity.Mathematics.Random>(rng, Allocator.TempJob);
            new ResolveCollisionJob()
            {
                fireParticleCollisionPair = fireParticleCollisionPair,
                fireParticles = fireParticles,
                rngRef = rngRef,
                settings = settings
            }.Run();
            rng = rngRef.Value;
            rngRef.Dispose();

            new ResolveTemperatureTransferJob()
            {
                fireParticleCollisionPair = fireParticleCollisionPair,
                fireParticles = fireParticles,
                settings = settings,
            }.Run();
        }

        public void Dispose()
        {
            fireParticleCollisionPair.Dispose();
        }
    }
}