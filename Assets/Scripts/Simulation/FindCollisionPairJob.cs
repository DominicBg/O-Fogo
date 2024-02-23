using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace OFogo
{
    [BurstCompile]
    public struct FindCollisionPairJob : IJob
    {
        [ReadOnly]
        public NativeArray<FireParticle> fireParticles;
        public SimulationSettings settings;

        [ReadOnly]
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

        [WriteOnly]
        public NativeList<FireParticleCollision> fireParticleCollisionPair;

        public void Execute()
        {
            NativeList<FireParticleCollision> collisionBuffer = new NativeList<FireParticleCollision>(16, Allocator.Temp);
            for (int i = 0; i < fireParticles.Length; i++)
            {
                int2 hash = OFogoHelper.HashPosition(fireParticles[i].position, in settings.simulationBound, settings.hashingGridLength);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        OFogoHelper.CheckCollisionPair(hash, i, x, y, fireParticles, nativeHashingGrid, settings, ref collisionBuffer);
                    }
                }
            }
            fireParticleCollisionPair.AddRange(collisionBuffer);
            collisionBuffer.Dispose();
        }
    }
}