using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace OFogo
{
    [BurstCompile]
    public struct FindCollisionPairParallelJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<FireParticle> fireParticles;
        public SimulationSettings settings;

        [ReadOnly]
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

        [WriteOnly]
        public NativeList<FireParticleCollision>.ParallelWriter fireParticleCollisionPair;

        public void Execute(int index)
        {
            NativeList<FireParticleCollision> collisionBuffer = new NativeList<FireParticleCollision>(200, Allocator.Temp);

            OFogoHelper.CheckCollisionPairAtPosition(index, fireParticles, nativeHashingGrid, settings, ref collisionBuffer);

            fireParticleCollisionPair.AddRangeNoResize(collisionBuffer);
            collisionBuffer.Dispose();
        }
    }
}