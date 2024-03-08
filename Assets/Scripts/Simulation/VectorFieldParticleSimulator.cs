using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class VectorFieldParticleSimulator : FireParticleSimulator
    {
        public override bool CanResolveCollision() => true;
        public override bool IsHandlingParticleHeating() => true;
        public override bool NeedsVectorField() => true;

        [System.Serializable]
        public struct InternalSettings
        {
            public IntegrationType integrationType;

            public float separationForce;
            public int maxCollision;
            public float wallBounceIntensity;
            public float maxSpeed;
            public static InternalSettings Default => new InternalSettings()
            {
                integrationType = IntegrationType.Verlet,
                maxCollision = -1,
                maxSpeed = 1,
                separationForce = 0.5f,
                wallBounceIntensity = 0.2f
            };
        }
        [SerializeField] InternalSettings internalSettings = InternalSettings.Default;

        NativeArray<float3> desiredForce;

        protected override void Init(in SimulationSettings settings)
        {
            desiredForce = new NativeArray<float3>(settings.particleCount, Allocator.Persistent);
        }

        public override void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            new ParticleSimulationJob()
            {
                fireParticles = fireParticles,
                simulationData = simulationData,
                vectorField = vectorField,
                settings = settings,
                internalSettings = internalSettings,
            }.RunParralelAndProfile(fireParticles.Length);

        }

        public override void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            new ParticleSimulationFindDesiredForceJob()
            {
                fireParticles = fireParticles,
                simulationData = simulationData,
                settings = settings,
                nativeHashingGrid = nativeHashingGrid,
                
                vectorField = vectorField,
                desiredForce = desiredForce
            }.RunParralelAndProfile(fireParticles.Length);

            new ParticleSimulationApplyDesiredForceJob()
            {
                fireParticles = fireParticles,
                settings = settings,
                desiredForce = desiredForce,
                
            }.RunParralelAndProfile(fireParticles.Length);
        }

        public override void Dispose()
        {
            desiredForce.Dispose();
        }

        [BurstCompile]
        public struct ParticleSimulationJob : IJobParallelFor
        {
            public SimulationData simulationData;

            public NativeArray<FireParticle> fireParticles;
            public SimulationSettings settings;
            public InternalSettings internalSettings;

            [ReadOnly]
            public NativeGrid<float3> vectorField;

            public void Execute(int index)
            {
                FireParticle fireParticle = fireParticles[index];
                float3 vectorFieldForce = vectorField[OFogoHelper.HashPosition(fireParticle.position, settings.simulationBound, vectorField.Size)];

                switch (internalSettings.integrationType)
                {
                    case IntegrationType.Euler:

                        fireParticle.velocity += vectorFieldForce * simulationData.dt;
                        if (math.lengthsq(fireParticle.velocity) > OFogoHelper.Pow2(internalSettings.maxSpeed))
                        {
                            fireParticle.velocity = math.normalize(fireParticle.velocity) * internalSettings.maxSpeed;
                        }
                        fireParticle.position += fireParticle.velocity * simulationData.dt;

                        break;
                    case IntegrationType.Verlet:

                        float3 velocity = fireParticle.position - fireParticle.prevPosition;
                        if (math.lengthsq(velocity) > OFogoHelper.Pow2(internalSettings.maxSpeed))
                        {
                            velocity = math.normalize(velocity) * internalSettings.maxSpeed;
                        }
                        fireParticle.prevPosition = fireParticle.position;
                        fireParticle.position += velocity + vectorFieldForce * simulationData.dt * simulationData.dt;
                        break;
                }

                var rng = Unity.Mathematics.Random.CreateFromIndex((uint)index);

                //stupid heat hack
                fireParticle.temperature = math.lerp(fireParticle.temperature, rng.NextFloat() * settings.maxTemperature, simulationData.dt * 10);

                fireParticles[index] = fireParticle;
            }
        }

        [BurstCompile]
        public struct ParticleSimulationFindDesiredForceJob : IJobParallelFor
        {
            public SimulationData simulationData;

            [ReadOnly]
            public NativeArray<FireParticle> fireParticles;
            public NativeArray<float3> desiredForce;

            public SimulationSettings settings;
            public InternalSettings internalSettings;
    
            [ReadOnly]
            public NativeGrid<UnsafeList<int>> nativeHashingGrid;

            [ReadOnly]
            public NativeGrid<float3> vectorField;

            public void Execute(int i)
            {
                var collisionBuffer = new NativeList<FireParticleCollision>(internalSettings.maxCollision, Allocator.Temp);
                OFogoHelper.CheckCollisionPairAtPosition(i, fireParticles, nativeHashingGrid, settings, ref collisionBuffer, internalSettings.maxCollision);

                float3 sperationDirectionSum = 0;
                for (int j = 0; j < collisionBuffer.Length; j++)
                {
                    int indexA = collisionBuffer[j].indexA;
                    int indexB = collisionBuffer[j].indexB;

                    float3 dir;

                    if (collisionBuffer[j].distSq < 0.001f)
                    {
                        var rng = Unity.Mathematics.Random.CreateFromIndex((uint)i);
                        dir = rng.NextFloat3Direction();
                    }
                    else
                    {
                        float dist = math.sqrt(collisionBuffer[j].distSq);
                        dir = (fireParticles[indexB].position - fireParticles[indexA].position) / dist;
                    }

                    sperationDirectionSum += dir;
                }

                if (collisionBuffer.Length > 0)
                {
                    desiredForce[i] = (sperationDirectionSum / collisionBuffer.Length) * internalSettings.separationForce * simulationData.dt;
                }
                else
                {
                    desiredForce[i] = 0;
                }
                collisionBuffer.Dispose();
            }
        }
    }

    [BurstCompile]
    public struct ParticleSimulationApplyDesiredForceJob : IJobParallelFor
    {
        public NativeArray<FireParticle> fireParticles;
        public NativeArray<float3> desiredForce;
        public SimulationSettings settings;
        public float wallBounceIntensity;

        public void Execute(int i)
        {
            FireParticle fireParticle = fireParticles[i];
            fireParticle.position += desiredForce[i];
            OFogoHelper.ApplyConstraintBounce(ref fireParticle, settings, wallBounceIntensity);
            fireParticles[i] = fireParticle;
        }
    }
}