using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace OFogo
{
    [BurstCompile]
    public struct UpdateSimulationJob : IJobParallelFor
    {
        public SimulationData simulationData;
        public NativeArray<FireParticle> fireParticles;
        public SimulationSettings settings;

        [ReadOnly]
        public NativeGrid<float3> vectorField;

        public void Execute(int index)
        {
            FireParticle fireParticle = fireParticles[index];

            fireParticle.temperature -= settings.temperatureDropPerSec * simulationData.dt;
            fireParticle.temperature = math.clamp(fireParticle.temperature, 0, settings.maxTemperature);
            fireParticle.radius = math.lerp(settings.minParticleSize, settings.maxParticleSize, fireParticle.temperature / settings.maxTemperature);

            float3 heatTurbulence = vectorField[OFogoHelper.HashPosition(fireParticle.position, settings.simulationBound, vectorField.Size)];
            float3 acceleration = 0;

            if (settings.useVectorFieldAsGravity)
            {
                acceleration += heatTurbulence * (settings.fireGravity + fireParticle.temperature * settings.temperatureUpwardForce);
            }
            else
            {
                acceleration += math.up() * (settings.fireGravity + fireParticle.temperature * settings.temperatureUpwardForce);
                acceleration += heatTurbulence;
            }

            switch (settings.integrationType)
            {
                case IntegrationType.Euler:

                    fireParticle.velocity += acceleration * simulationData.dt;
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
                    fireParticle.position += velocity + acceleration * simulationData.dt * simulationData.dt;
                    break;
            }

            OFogoHelper.ApplyConstraintBounce(ref fireParticle, settings);
            fireParticles[index] = fireParticle;
        }
    }
}