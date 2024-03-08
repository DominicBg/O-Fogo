using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace OFogo
{
    [BurstCompile]
    public class OFogoSimulator : FireParticleSimulator
    {
        [System.Serializable]
        public struct InternalSettings
        {
            public IntegrationType integrationType;
            public uint seed;
            public int maxParticleCollision;
            public bool parallelCollision;

            public float resolutionStepRatio;
            public float collisionVelocityResolution;
            public float maxSpeed;
            public float wallBounceIntensity;
            public bool useVectorFieldAsGravity;
            public float fireGravity;
            public float temperatureDropPerSec;
            public float temperatureUpwardForce;
            public float heatTransferPercent;

            public static InternalSettings Default = new InternalSettings()
            {
                integrationType = IntegrationType.Verlet,
                seed = 43243215,
                maxParticleCollision = -1,
                parallelCollision = true,
                resolutionStepRatio = 0.5f,
                collisionVelocityResolution = 0.2f,
                maxSpeed = 1,
                fireGravity = -3,
                heatTransferPercent = 0.5f,
                temperatureDropPerSec = 1,
                temperatureUpwardForce = 3,
                useVectorFieldAsGravity = false,
                wallBounceIntensity = 0.2f
            };
        }

        public override bool CanResolveCollision() => true;
        public override bool IsHandlingParticleHeating() => false;
        public override bool NeedsVectorField() => true;
        public InternalSettings internalSettings = InternalSettings.Default;

        NativeList<FireParticleCollision> fireParticleCollisionPair;
        Random rng;

        protected override void Init(in SimulationSettings settings)
        {
            rng = Random.CreateFromIndex(internalSettings.seed);
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

        public override void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            new UpdateSimulationJob()
            {
                simulationData = simulationData,
                fireParticles = fireParticles,
                settings = settings,
                internalSettings = internalSettings,
                vectorField = vectorField,
            }.RunParralelAndProfile(fireParticles.Length);
        }

        public override void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            fireParticleCollisionPair.Clear();
            if (internalSettings.parallelCollision)
            {
                new FindCollisionPairParallelJob()
                {
                    fireParticles = fireParticles,
                    fireParticleCollisionPair = fireParticleCollisionPair.AsParallelWriter(),
                    nativeHashingGrid = nativeHashingGrid,
                    settings = settings,
                    internalSettings = internalSettings
                }.RunParralelAndProfile(fireParticles.Length);
            }
            else
            {
                new FindCollisionPairJob()
                {
                    fireParticles = fireParticles,
                    fireParticleCollisionPair = fireParticleCollisionPair,
                    nativeHashingGrid = nativeHashingGrid,
                    settings = settings,
                    internalSettings = internalSettings
                }.RunAndProfile();
            }

            var rngRef = new NativeReference<Unity.Mathematics.Random>(rng, Allocator.TempJob);
            new ResolveCollisionJob()
            {
                fireParticleCollisionPair = fireParticleCollisionPair,
                fireParticles = fireParticles,
                rngRef = rngRef,
                settings = settings,
                internalSettings = internalSettings
            }.RunAndProfile();

            rng = rngRef.Value;
            rngRef.Dispose();

            new ResolveTemperatureTransferJob()
            {
                fireParticleCollisionPair = fireParticleCollisionPair,
                fireParticles = fireParticles,
                settings = settings,
            }.RunAndProfile();
        }

        public override void Dispose()
        {
            fireParticleCollisionPair.Dispose();
        }
    }
}