using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class VectorFieldParticleSimulation : MonoBehaviour
    {
        public float separationForce;
        public int maxCollision = 100;

        NativeArray<float3> desiredForce;

        public void Init(int particleCount)
        {
            desiredForce = new NativeArray<float3>(particleCount, Allocator.Persistent);
        }

        public void TickSimulation(in SimulationData simulationData, NativeArray<FireParticle> fireParticles, NativeGrid<float3> vectorField, NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            new ParticleSimulationJob()
            {
                fireParticles = fireParticles,
                simulationData = simulationData,
                vectorField = vectorField,
                settings = settings,
                maxCollision = maxCollision,
            }.Schedule(fireParticles.Length, fireParticles.Length / 16).Complete();

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
            }.Schedule(fireParticles.Length, fireParticles.Length / 16).Complete();

            new ParticleSimulationApplyDesiredForceJob()
            {
                fireParticles = fireParticles,
                settings = settings,
                desiredForce = desiredForce
            }.Schedule(fireParticles.Length, fireParticles.Length / 16).Complete();
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
                    float dist = math.sqrt(collisionBuffer[j].distSq);
                    float3 dir = (fireParticles[indexA].position - fireParticles[indexB].position) / dist;
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