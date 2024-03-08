using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using static OFogo.OFogoSimulator;

namespace OFogo
{
    [BurstCompile]
    public struct FindCollisionPairJob : IJob
    {
        [ReadOnly]
        public NativeArray<FireParticle> fireParticles;
        public SimulationSettings settings;
        public InternalSettings internalSettings;

        [ReadOnly]
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

        [WriteOnly]
        public NativeList<FireParticleCollision> fireParticleCollisionPair;

        public void Execute()
        {
            for (int i = 0; i < fireParticles.Length; i++)
            {
                OFogoHelper.CheckCollisionPairAtPosition(i, fireParticles, nativeHashingGrid, settings, ref fireParticleCollisionPair, internalSettings.maxParticleCollision);
            }
        }
    }
}