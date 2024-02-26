using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    [BurstCompile]
    public class OFogoSimulator : MonoBehaviour
    {
        public float initialSpacing = 0.5f;
        public int particleCount;
        public uint seed = 43243215;
        public bool parallelCollision;

        public SimulationSettings settings;
        public NativeGrid<float3> vectorField;
        public NativeArray<FireParticle> fireParticles;

        NativeList<FireParticleCollision> fireParticleCollisionPair;
        NativeGrid<UnsafeList<int>> nativeHashingGrid;
        Unity.Mathematics.Random rng;

        public void Init()
        {
            rng = Unity.Mathematics.Random.CreateFromIndex(seed);

            fireParticles = new NativeArray<FireParticle>(particleCount, Allocator.Persistent);
            fireParticleCollisionPair = new NativeList<FireParticleCollision>(GetMaxCollisionCount(), Allocator.Persistent);
            nativeHashingGrid = new NativeGrid<UnsafeList<int>>(settings.hashingGridLength, Allocator.Persistent);

            for (int x = 0; x < settings.hashingGridLength.x; x++)
            {
                for (int y = 0; y < settings.hashingGridLength.y; y++)
                {
                    nativeHashingGrid[x, y] = new UnsafeList<int>(32, Allocator.Persistent);
                }
            }

            int particlePerCol = (int)math.sqrt(particleCount);
            for (int i = 0; i < particleCount; i++)
            {
                int2 xy = new int2(i % particlePerCol, i / particlePerCol);
                float3 pos = new float3((float2)xy * initialSpacing, 0f);
                pos.x += (xy.y % 2 == 0) ? 0.5f * initialSpacing : 0f;

                FireParticle fireParticle = new FireParticle()
                {
                    position = pos,
                    prevPosition = pos,
                    radius = settings.minParticleSize,
                    temperature = 0,
                    velocity = 0
                };
                fireParticles[i] = fireParticle;
            }
        }

        private int GetMaxCollisionCount()
        {
            // each particle can collid with every other particle n^2
            // if i hit j, we skip j hit i
            // n(n+1)/2 
            // we remove self particle hit
            return (particleCount * (particleCount - 1)) / 2;
        }

        public void TickSimulation(in SimulationData simData)
        {
            UpdateSimulation(in simData);
        }

        void UpdateSimulation(in SimulationData simulationData)
        {
            new UpdateSimulationJob()
            {
                simulationData = simulationData,
                fireParticles = fireParticles,
                settings = settings,
                vectorField = vectorField
            }.Schedule(fireParticles.Length, fireParticles.Length / 16).Complete();

            fireParticleCollisionPair.Clear();

            for (int x = 0; x < settings.hashingGridLength.x; x++)
            {
                for (int y = 0; y < settings.hashingGridLength.y; y++)
                {
                    var list = nativeHashingGrid[x, y];
                    list.Clear();
                    nativeHashingGrid[x, y] = list;
                }
            }

            for (int i = 0; i < fireParticles.Length; i++)
            {
                int2 hash = OFogoHelper.HashPosition(fireParticles[i].position, in settings.simulationBound, settings.hashingGridLength);

                var list = nativeHashingGrid[hash];
                list.Add(i);
                nativeHashingGrid[hash] = list;
            }

            if (parallelCollision)
            {
                new FindCollisionPairParallelJob()
                {
                    fireParticles = fireParticles,
                    fireParticleCollisionPair = fireParticleCollisionPair.AsParallelWriter(),
                    nativeHashingGrid = nativeHashingGrid,
                    settings = settings
                }.Schedule(fireParticles.Length, fireParticles.Length / 16).Complete();
            }
            else
            {
                new FindCollisionPairJob()
                {
                    fireParticles = fireParticles,
                    fireParticleCollisionPair = fireParticleCollisionPair,
                    nativeHashingGrid = nativeHashingGrid,
                    settings = settings
                }.Run();
            }

            var rngRef = new NativeReference<Unity.Mathematics.Random>(rng, Allocator.TempJob);
            new ResolveCollisionJob()
            {
                fireParticleCollisionPair = fireParticleCollisionPair,
                fireParticles = fireParticles,
                rngRef = rngRef,
                settings = settings
            }.Run();
            rng = rngRef.Value;

            new ResolveTemperatureTransferJob()
            {
                fireParticleCollisionPair = fireParticleCollisionPair,
                fireParticles = fireParticles,
                settings = settings,
            }.Run();
        }


        public void Dispose()
        {            
            if (fireParticles.IsCreated)
            {
                fireParticles.Dispose();
                fireParticleCollisionPair.Dispose();

                for (int x = 0; x < settings.hashingGridLength.x; x++)
                {
                    for (int y = 0; y < settings.hashingGridLength.y; y++)
                    {
                        nativeHashingGrid[x, y].Dispose();
                    }
                }
                nativeHashingGrid.Dispose();
            }
        }
    }
}