using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class ElPisoEsLava : Calentador
    {
        [System.Serializable]
        public struct HeatSettings
        {
            public float heatAtBottomHeight;
            [Range(0, 1)]
            public float heatAtBottomNoiseRatio;
            public float heatAtBottomNoiseSize;
            public float heatAtBottomNoiseSpeed;
        }

        public HeatSettings heatSettings = new HeatSettings()
        {
            heatAtBottomHeight = 0.5f,
            heatAtBottomNoiseRatio = 0.5f,
            heatAtBottomNoiseSize = 1,
            heatAtBottomNoiseSpeed = 1,
        };

        public override void HeatParticles(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in SimulationSettings settings)
        {
            new ElPisoEsLavaJob()
            {
                simData = simulationData,
                fireParticles = fireParticles,
                settings = settings,
                heatSettings = heatSettings,
            }.Schedule(fireParticles.Length, fireParticles.Length / 8).Complete();
        }

        [BurstCompile]
        public struct ElPisoEsLavaJob : IJobParallelFor, ICalentador
        {
            public SimulationData simData;
            public NativeArray<FireParticle> fireParticles;
            public SimulationSettings settings;
            public HeatSettings heatSettings;
            
            public void Execute(int index)
            {
                FireParticle fireParticleParticle = fireParticles[index];
                WarmupParticle(ref fireParticleParticle);
                fireParticles[index] = fireParticleParticle;
            }

            public void WarmupParticle(ref FireParticle particle)
            {
                float minSimulationY = simData.pos.y + settings.simulationBound.min.y;
                float heightSmoothStep = 1f - math.smoothstep(minSimulationY, minSimulationY + heatSettings.heatAtBottomHeight, particle.position.y);

                if (heatSettings.heatAtBottomNoiseRatio > 0)
                {
                    float heatNoise = noise.cnoise(new float2(simData.time, particle.position.x * heatSettings.heatAtBottomNoiseSize));
                    heightSmoothStep *= math.lerp(1, heatNoise, heatSettings.heatAtBottomNoiseRatio);
                }

                float heat = heightSmoothStep * settings.heatingPerSec;
                particle.temperature += heat * simData.dt;
            }
        }

        public override void DrawDebug(in SimulationData simData,in SimulationSettings settings)
        {
            base.DrawDebug(simData, settings);
            float3 min = simData.pos + (float3)settings.simulationBound.min;
            float3 max = simData.pos + (float3)settings.simulationBound.max;
            float3 bottomLeft = new float3(min.x, min.y, 0f);
            float3 bottomRight = new float3(max.x, min.y, 0f);
            float3 heatLeft = bottomLeft + math.up() * heatSettings.heatAtBottomHeight;
            float3 heatRight = bottomRight + math.up() * heatSettings.heatAtBottomHeight;
            Debug.DrawLine(heatLeft, heatRight, Color.red);
        }
    }

}