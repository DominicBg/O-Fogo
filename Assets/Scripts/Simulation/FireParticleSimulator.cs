using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public abstract class FireParticleSimulator : MonoBehaviour
    {
        bool isInit;
        public void TryInit(in SimulationSettings settings)
        {
            if(!isInit)
            {
                Init(settings);
                isInit = true;
            }
        }

        protected abstract void Init(in SimulationSettings settings);
        public abstract void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings);
        public abstract void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings);
        public abstract void Dispose();

        public abstract bool CanResolveCollision();
        public abstract bool IsHandlingParticleHeating();
        public abstract bool NeedsVectorField();
    }
}