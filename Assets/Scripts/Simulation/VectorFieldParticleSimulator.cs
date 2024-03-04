using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

namespace OFogo
{
    public class VectorFieldParticleSimulator : MonoBehaviour, IFireParticleSimulator
    {
        public bool CanResolveCollision() => true;
        public bool IsHandlingParticleHeating() => true;
        public bool NeedsVectorField() => true;

        public float separationForce;
        public int maxCollision = 100;

        NativeArray<float3> desiredForce;

        public void Init(in SimulationSettings settings)
        {
            desiredForce = new NativeArray<float3>(settings.particleCount, Allocator.Persistent);
        }

        public void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            new ParticleSimulationJob()
            {
                fireParticles = fireParticles,
                simulationData = simulationData,
                vectorField = vectorField,
                settings = settings,
                maxCollision = maxCollision,
            }.RunParralelAndProfile(fireParticles.Length);

        }

        public void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            new ParticleSimulationFindDesiredForceJob()
            {
                fireParticles = fireParticles,
                simulationData = simulationData,
                settings = settings,
                nativeHashingGrid = nativeHashingGrid,
                maxCollision = maxCollision,
                separationForce = separationForce,
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

        public void Dispose()
        {
            desiredForce.Dispose();
        }

        [BurstCompile]
        public struct ParticleSimulationJob : IJobParallelFor
        {
            public SimulationData simulationData;

            public NativeArray<FireParticle> fireParticles;
            public SimulationSettings settings;
            public float separationForce;
            public int maxCollision;

            [ReadOnly]
            public NativeGrid<float3> vectorField;

            public void Execute(int index)
            {
                FireParticle fireParticle = fireParticles[index];
                float3 vectorFieldForce = vectorField[OFogoHelper.HashPosition(fireParticle.position, settings.simulationBound, vectorField.Size)];

                switch (settings.integrationType)
                {
                    case IntegrationType.Euler:

                        fireParticle.velocity += vectorFieldForce * simulationData.dt;
                        if (math.lengthsq(fireParticle.velocity) > OFogoHelper.Pow2(settings.maxSpeed))
                        {
                            fireParticle.velocity = math.normalize(fireParticle.velocity) * settings.maxSpeed;
                        }
                        fireParticle.position += fireParticle.velocity * simulationData.dt;

                        break;
                    case IntegrationType.Verlet:

                        float3 velocity = fireParticle.position - fireParticle.prevPosition;
                        if (math.lengthsq(velocity) > OFogoHelper.Pow2(settings.maxSpeed))
                        {
                            velocity = math.normalize(velocity) * settings.maxSpeed;
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
            public float separationForce;
            public int maxCollision;

            [ReadOnly]
            public NativeGrid<UnsafeList<int>> nativeHashingGrid;

            [ReadOnly]
            public NativeGrid<float3> vectorField;

            public void Execute(int i)
            {
                var collisionBuffer = new NativeList<FireParticleCollision>(maxCollision, Allocator.Temp);
                OFogoHelper.CheckCollisionPairAtPosition(i, fireParticles, nativeHashingGrid, settings, ref collisionBuffer, maxCollision);

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
                    desiredForce[i] = (sperationDirectionSum / collisionBuffer.Length) * separationForce * simulationData.dt;
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

        public void Execute(int i)
        {
            FireParticle fireParticle = fireParticles[i];
            fireParticle.position += desiredForce[i];
            OFogoHelper.ApplyConstraintBounce(ref fireParticle, settings);
            fireParticles[i] = fireParticle;
        }
    }
}