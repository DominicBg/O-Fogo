using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace OFogo
{
    [BurstCompile]
    public struct FillHashGridJob : IJob
    {
        public NativeArray<FireParticle> fireParticles;
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;
        public SimulationSettings settings;

        public FillHashGridJob(NativeArray<FireParticle> fireParticles, NativeGrid<UnsafeList<int>> nativeHashingGrid, SimulationSettings settings)
        {
            this.fireParticles = fireParticles;
            this.nativeHashingGrid = nativeHashingGrid;
            this.settings = settings;
        }

        public void Execute()
        {
            for (int x = 0; x < nativeHashingGrid.Size.x; x++)
            {
                for (int y = 0; y < nativeHashingGrid.Size.y; y++)
                {
                    var list = nativeHashingGrid[x, y];
                    list.Clear();
                    nativeHashingGrid[x, y] = list;
                }
            }

            for (int i = 0; i < fireParticles.Length; i++)
            {
                int2 hash = OFogoHelper.HashPosition(fireParticles[i].position, in settings.simulationBound, nativeHashingGrid.Size);

                var list = nativeHashingGrid[hash];
                list.Add(i);
                nativeHashingGrid[hash] = list;
            }
        }
    }
}