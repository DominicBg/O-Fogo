using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class SimulatorBlend : FireParticleSimulator
    {
        public FireParticleSimulator fireParticleSimulatorA;
        public FireParticleSimulator fireParticleSimulatorB;
        public bool isFireSimulatorLerpAdditive;
        [Range(0, 1)]
        public float ratio = 0;

        public override bool CanResolveCollision() => false; //Handled internally
        public override bool IsHandlingParticleHeating() => false; //Handled internally
        public override bool NeedsVectorField() => false; //Handled internally

        public NativeArray<FireParticle> fireParticlesB;

        protected override void Init(in SimulationSettings settings)
        {
            fireParticlesB = new NativeArray<FireParticle>(settings.particleCount, Allocator.Persistent);
        }

        public override void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            if(ratio == 0)
            {
                OFogoController.Instance.UpdateSimulation(fireParticleSimulatorA, simulationData, fireParticles);
            }
            else if(ratio == 1)
            {
                OFogoController.Instance.UpdateSimulation(fireParticleSimulatorB, simulationData, fireParticles);
            }
            else
            {
                OFogoController.Instance.UpdateSimulation(fireParticleSimulatorA, simulationData, fireParticles);
                OFogoController.Instance.UpdateSimulation(fireParticleSimulatorB, simulationData, fireParticlesB);
                new LerpParticleJobs(fireParticles, fireParticlesB, ratio, isFireSimulatorLerpAdditive).RunParralelAndProfile(fireParticles.Length);
            }
        }

        public struct LerpParticleJobs : IJobParallelFor
        {
            NativeArray<FireParticle> fireParticlesA;
            NativeArray<FireParticle> fireParticlesB;
            public float t;
            public bool isFireSimulatorLerpAdditive;

            public LerpParticleJobs(NativeArray<FireParticle> fireParticlesA, NativeArray<FireParticle> fireParticlesB, float t, bool isFireSimulatorLerpAdditive)
            {
                this.fireParticlesA = fireParticlesA;
                this.fireParticlesB = fireParticlesB;
                this.isFireSimulatorLerpAdditive = isFireSimulatorLerpAdditive;
                this.t = t;
            }

            public void Execute(int index)
            {
                FireParticle fireParticle = FireParticle.Lerp(fireParticlesA[index], fireParticlesB[index], t);
                fireParticlesA[index] = fireParticle;

                //if not, it won't reset the position of the particles B, so it will do a drag effect
                if (!isFireSimulatorLerpAdditive)
                {
                    fireParticlesB[index] = fireParticle;
                }
            }
        }

        public override void Dispose()
        {
            fireParticlesB.Dispose();
        }

        public override void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings)
        {
            //
        }
    }
}