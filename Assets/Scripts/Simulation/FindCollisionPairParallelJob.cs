using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

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
            NativeList<FireParticleCollision> collisionBuffer = new NativeList<FireParticleCollision>(16, Allocator.Temp);
            int2 hash = OFogoHelper.HashPosition(fireParticles[index].position, in settings.simulationBound, settings.hashingGridLength);

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    OFogoHelper.CheckCollisionPair(hash, index, x, y, fireParticles, nativeHashingGrid, settings, ref collisionBuffer);
                }
            }
            fireParticleCollisionPair.AddRangeNoResize(collisionBuffer);
            collisionBuffer.Dispose();
        }
    }
}