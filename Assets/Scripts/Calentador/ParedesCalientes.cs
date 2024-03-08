using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class ParedesCalientes : Calentador
    {
        [System.Serializable]
        public struct HeatSettings
        {
            public float heatSize;
            public float heatingPerSec;

            public static HeatSettings Default = new HeatSettings()
            {
                heatSize = 0.5f,
                heatingPerSec = 10,
            };
        }

        [SerializeField] HeatSettings heatSettings = HeatSettings.Default;

        public override void HeatParticles(in SimulationData simData, ref NativeArray<FireParticle> fireParticles, in SimulationSettings settings)
        {
            new ParedesCalientesJob()
            {
                simData = simData,
                fireParticles = fireParticles,
                settings = settings,
                heatSettings = heatSettings,
            }.RunParralelAndProfile(fireParticles.Length);
        }

        [BurstCompile]
        public struct ParedesCalientesJob : ICalentador, IJobParallelFor
        {
            public NativeArray<FireParticle> fireParticles;
            public SimulationSettings settings;

            public SimulationData simData;
            public HeatSettings heatSettings;

            public void Execute(int index)
            {
                FireParticle fireParticleParticle = fireParticles[index];
                HeatParticle(ref fireParticleParticle);
                fireParticles[index] = fireParticleParticle;
            }

            public void HeatParticle(ref FireParticle particle)
            {
                float3 min = settings.simulationBound.min;
                float3 max = settings.simulationBound.max;

                float2 distPosMinBound = math.distance(particle.position.xy, min.xy);
                float2 distPosMaxBound = math.distance(particle.position.xy, max.xy);
                float2 minDists = math.min(distPosMinBound, distPosMaxBound);
                float minDist = math.cmin(minDists);

                float heightSmoothStep = 1f - math.smoothstep(0, heatSettings.heatSize, minDist);
                float heat = heightSmoothStep * heatSettings.heatingPerSec;
                particle.temperature += heat * simData.dt;
            }
        }
        public override void DrawDebug(float3 simPosition, in SimulationSettings settings)
        {
            base.DrawDebug(simPosition, settings);
            float3 min = simPosition + (float3)settings.simulationBound.min;
            float3 max = simPosition + (float3)settings.simulationBound.max;

            float3 bottomLeft = new float3(min.x, min.y, 0f);
            float3 bottomRight = new float3(max.x, min.y, 0f);
            float3 topLeft = new float3(min.x, max.y, 0f);
            float3 topRight = new float3(max.x, max.y, 0f);

            Debug.DrawLine(
                bottomLeft + math.up() * heatSettings.heatSize,
                bottomRight + math.up() * heatSettings.heatSize,
                Color.red);
            
            Debug.DrawLine(
                topLeft + math.down() * heatSettings.heatSize,
                topRight + math.down() * heatSettings.heatSize,
                Color.red);

            Debug.DrawLine(
                bottomLeft + math.right() * heatSettings.heatSize,
                topLeft + math.right() * heatSettings.heatSize,
                Color.red);

            Debug.DrawLine(
              bottomRight + math.left() * heatSettings.heatSize,
              topRight + math.left() * heatSettings.heatSize,
              Color.red);
        }
    }

}