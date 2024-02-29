using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class TriangleVectorFieldRenderer : VectorFieldRenderer
    {
        [SerializeField] ParticleSystem ps;
        [SerializeField] float particleScaleMultiplier = 1;
        [SerializeField] float maxForce = 1;
        [SerializeField] float offsetRotationDeg;
        [SerializeField] Color baseColor;

        NativeArray<ParticleSystem.Particle> renderParticles;

        public override void Init(in NativeGrid<float3> vectorField)
        {
            var main = ps.main;
            main.maxParticles = vectorField.TotalLength;

            var emission = ps.emission;
            emission.enabled = false;

            renderParticles = new NativeArray<ParticleSystem.Particle>(vectorField.TotalLength, Allocator.Persistent);
        }


        public override void Render(in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            new CalculateParticleDataJob()
            {
                renderParticles = renderParticles,
                settings = settings,
                vectorField = vectorField,
                particleScaleMultiplier = particleScaleMultiplier,
                maxForce = maxForce,
                offsetRotationDeg = offsetRotationDeg,
                baseColor = baseColor
            }.RunParralel(renderParticles.Length);

            ps.SetParticles(renderParticles);
        }

        [BurstCompile]
        public struct CalculateParticleDataJob : IJobParallelFor
        {
            public NativeArray<ParticleSystem.Particle> renderParticles;
            [ReadOnly] public NativeGrid<float3> vectorField;
            public SimulationSettings settings;
            public float particleScaleMultiplier;
            public float maxForce;
            public float offsetRotationDeg;
            public Color baseColor;

            public void Execute(int i)
            {
                ParticleSystem.Particle particle = renderParticles[i];

                int2 pos2 = vectorField.IndexToPos(i);
                float2 invSize = 1f / (float2)vectorField.Size;
                float3 min = settings.simulationBound.min;
                float3 max = settings.simulationBound.max;

                float2 pos = new float2(pos2.x, pos2.y) * invSize;
                float3 t = new float3(pos, 0);
                float3 posInCell = math.lerp(min, max, t);
                float3 force = vectorField[i];

                float forceLength = math.length(force);
                float3 dir = force / forceLength;

                particle.position = posInCell;
                particle.startSize = particleScaleMultiplier;
                particle.rotation = math.degrees(math.atan2(dir.y, dir.x)) + offsetRotationDeg;

                particle.startColor = baseColor * math.saturate(forceLength / maxForce);

                renderParticles[i] = particle;
            }
        }

        public override void Dispose()
        {
            renderParticles.Dispose();
        }
    }
}