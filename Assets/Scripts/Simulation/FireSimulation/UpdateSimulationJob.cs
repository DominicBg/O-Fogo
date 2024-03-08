using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using static OFogo.OFogoSimulator;

namespace OFogo
{
    [BurstCompile]
    public struct UpdateSimulationJob : IJobParallelFor
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

            fireParticle.temperature -= internalSettings.temperatureDropPerSec * simulationData.dt;
            fireParticle.temperature = math.clamp(fireParticle.temperature, 0, settings.maxTemperature);
            fireParticle.radius = math.lerp(settings.minParticleSize, settings.maxParticleSize, fireParticle.temperature / settings.maxTemperature);

            float3 heatTurbulence = vectorField[OFogoHelper.HashPosition(fireParticle.position, settings.simulationBound, vectorField.Size)];
            float3 acceleration = 0;

            if (internalSettings.useVectorFieldAsGravity)
            {
                acceleration += math.normalizesafe(heatTurbulence) * (internalSettings.fireGravity + fireParticle.temperature * internalSettings.temperatureUpwardForce);
            }
            else
            {
                acceleration += math.up() * (internalSettings.fireGravity + fireParticle.temperature * internalSettings.temperatureUpwardForce);
                acceleration += heatTurbulence;
            }

            switch (internalSettings.integrationType)
            {
                case IntegrationType.Euler:

                    fireParticle.velocity += acceleration * simulationData.dt;
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
                    fireParticle.position += velocity + acceleration * simulationData.dt * simulationData.dt;
                    break;
            }

            OFogoHelper.ApplyConstraintBounce(ref fireParticle, settings, internalSettings.wallBounceIntensity);
            fireParticles[index] = fireParticle;
        }
    }
}