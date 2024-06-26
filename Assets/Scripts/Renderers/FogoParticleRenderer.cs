using Unity.Collections;
using UnityEngine;

namespace OFogo
{
    public class FogoParticleRenderer : FogoRenderer
    {
        [SerializeField] ParticleSystem ps;
        [SerializeField] Gradient heatGradient;
        [SerializeField] float particleScaleMultiplier = 1;

        NativeArray<ParticleSystem.Particle> renderParticles;

        public override void Init(int particleCount)
        {
            var main = ps.main;
            main.maxParticles = particleCount;

            var emission = ps.emission;
            emission.enabled = false;

            renderParticles = new NativeArray<ParticleSystem.Particle>(particleCount, Allocator.Persistent);
        }

        protected override void OnRender(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings)
        {
            for (int i = 0; i < renderParticles.Length; i++)
            {
                FireParticle fireParticle = fireParticles[i];
                ParticleSystem.Particle particle = renderParticles[i];
                particle.position = fireParticle.position;
                particle.startSize = fireParticle.radius * particleScaleMultiplier;
                Color particleColor = heatGradient.Evaluate(fireParticle.temperature / settings.maxTemperature);
                particleColor.a = alpha;
                particle.startColor = particleColor;
                renderParticles[i] = particle;
            }
            ps.SetParticles(renderParticles);
        }

        public override void Dispose()
        {
            if(renderParticles.IsCreated)
                renderParticles.Dispose();
        }

        public override void OnStopRendering(in NativeArray<FireParticle> fireParticles, in SimulationSettings settings)
        {
            for (int i = 0; i < renderParticles.Length; i++)
            {
                ParticleSystem.Particle particle = renderParticles[i];
                particle.startColor = Color.clear;
                renderParticles[i] = particle;
            }
            ps.SetParticles(renderParticles);
        }
    }
}