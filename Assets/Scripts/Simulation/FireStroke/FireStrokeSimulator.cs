using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class FireStrokeSimulator : FireParticleSimulator
    {
        public override bool CanResolveCollision() => false;
        public override bool IsHandlingParticleHeating() => true;
        public override bool NeedsVectorField() => false;

        public float timeScale = 1;
        public float burnSpeed = 1;
        public float burnHeight = 0.1f;
        public float noiseSpeed = 1f;
        public float noiseAmplitude = 0.1f;
        [Range(0, 1)] public float heatMultiplicator = 0.5f;

        FireStroke[] fireLines;

        NativeArray<FireStrokeContainer> fireStrokeContainers;
        NativeArray<ParticleInfoPerLine> particlesInfoPerLines;

        public struct ParticleInfoPerLine
        {
            public float length;
            public int startIndex;
            public int count;
        }

        protected override void Init(in SimulationSettings settings)
        {
            fireLines = GetComponentsInChildren<FireStroke>();
            fireStrokeContainers = new NativeArray<FireStrokeContainer>(fireLines.Length, Allocator.Persistent);
            particlesInfoPerLines = new NativeArray<ParticleInfoPerLine>(fireLines.Length, Allocator.Persistent);
        }

        public override void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            float lengthSum = 0;
            for (int i = 0; i < fireLines.Length; i++)
            {
                fireStrokeContainers[i] = fireLines[i].CreateFireStrokeContainer();
                float length = fireStrokeContainers[i].GetLength();
                lengthSum += length;
                particlesInfoPerLines[i] = new ParticleInfoPerLine()
                {
                    length = length
                };
            }

            int particleStartIndex = 0;
            for (int i = 0; i < fireLines.Length; i++)
            {
                var info = particlesInfoPerLines[i];
                float lengthRatio = info.length / lengthSum;
                info.count = (int)(lengthRatio * settings.particleCount);

                //with cast to int there might be overflow of particles
                if (particleStartIndex + info.count > settings.particleCount)
                {
                    //take the rest
                    info.count = settings.particleCount - particleStartIndex;
                }

                info.startIndex = particleStartIndex;
                particlesInfoPerLines[i] = info;

                particleStartIndex += info.count;
            }

            new ProcessFireLineJob()
            {
                timeScale = timeScale,
                settings = settings,
                fireParticles = fireParticles,
                noiseAmplitude = noiseAmplitude,
                noiseSpeed = noiseSpeed,
                heatMultiplicator = heatMultiplicator,
                burnSpeed = burnSpeed,
                burnHeight = burnHeight,

                time = Time.time,
                fireStrokeContainer = fireStrokeContainers,
                particlesInfoPerLines = particlesInfoPerLines
            }.RunParralelAndProfile(fireParticles.Length);
        }

        public override void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            //
        }

        public override void Dispose()
        {
            fireStrokeContainers.Dispose();
            particlesInfoPerLines.Dispose();
        }

        [BurstCompile]
        public struct ProcessFireLineJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<FireStrokeContainer> fireStrokeContainer;
            [ReadOnly]
            public NativeArray<ParticleInfoPerLine> particlesInfoPerLines;

            public SimulationSettings settings;
            public NativeArray<FireParticle> fireParticles;
            public float timeScale;
            public float noiseSpeed;
            public float noiseAmplitude;
            public float time;
            public float heatMultiplicator;
            public float burnSpeed;
            public float burnHeight;

            public void Execute(int index)
            {
                ParticleInfoPerLine infoPerLine = particlesInfoPerLines[0]; //prevent division per 0
                int fireLineId = 0;
                for (int j = 0; j < particlesInfoPerLines.Length; j++)
                {
                    if (index >= particlesInfoPerLines[j].startIndex && index < particlesInfoPerLines[j].startIndex + particlesInfoPerLines[j].count)
                    {
                        infoPerLine = particlesInfoPerLines[j];
                        fireLineId = j;
                        break;
                    }
                }

                FireParticle particle = fireParticles[index];

                int indexForLine = (index - infoPerLine.startIndex);
                float t = ((float)indexForLine / infoPerLine.count) + time * timeScale;
                t = math.frac(t);
                particle.position = fireStrokeContainer[fireLineId].Evaluate(t);

                float2 noiseValue = noiseAmplitude * new float2(
                    noise.snoise(new float2(time * noiseSpeed, index * 1.7283f)),
                    noise.snoise(new float2(time * noiseSpeed, index * 7.73816f + settings.particleCount))
                );

                noiseValue = noiseValue * 2 - 1;//normalize [0, 1] -> [-1, 1]

                particle.position += new float3(noiseValue.x, noiseValue.y, 0);

                var rng = Unity.Mathematics.Random.CreateFromIndex((uint)index);

                float heatRatio = rng.NextFloat();
                heatRatio += time * burnSpeed;
                heatRatio = math.frac(heatRatio);

                heatRatio *= heatMultiplicator;
                
                bool useRatioAsHeat = fireStrokeContainer[fireLineId].useRatioAsHeat;
                if(useRatioAsHeat)
                {
                    heatRatio *= t;
                }

                particle.prevPosition = particle.position;
                particle.position += math.up() * heatRatio * burnHeight;
                particle.temperature = heatRatio * settings.maxTemperature;

                particle.radius = math.lerp(settings.minParticleSize, settings.maxParticleSize, heatRatio);

                fireParticles[index] = particle;
            }
        }
    }
}