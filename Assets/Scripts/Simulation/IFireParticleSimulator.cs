using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace OFogo
{
    public interface IFireParticleSimulator
    {
        void Init(in SimulationSettings settings);
        void UpdateSimulation(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in SimulationSettings settings);
        void ResolveCollision(in SimulationData simulationData, ref NativeArray<FireParticle> fireParticles, in NativeGrid<float3> vectorField, in NativeGrid<UnsafeList<int>> nativeHashingGrid, in SimulationSettings settings);
        void Dispose();

        bool CanResolveCollision();
        bool IsHandlingParticleHeating();
        bool NeedsVectorField();
    }
}