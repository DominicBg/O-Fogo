using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    [BurstCompile]
    public class FogoSimulator : MonoBehaviour
    {
        [SerializeField] float initialSpacing = 0.5f; //move initiation in interface?
        [SerializeField] int particleCount;
        [SerializeField] uint seed = 43243215;
        [SerializeField] int substeps = 4;
        [SerializeField] float simulationSpeed = 1;
        [SerializeField] bool parallelCollision;

        [SerializeField] int2 vectorFieldSize = 35;
        [SerializeField] Calentador calentador;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] SimulationSettings settings;
        [SerializeField] float debugRayDist = 5;

        //public for the editor lol, this is stupid
        public NativeGrid<float3> vectorField;
        NativeArray<FireParticle> fireParticles;
        NativeList<FireParticleCollision> fireParticleCollisionPair;
        NativeGrid<UnsafeList<int>> nativeHashingGrid;
        SimulationData simData;
        Unity.Mathematics.Random rng;

        void Start()
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
            fogoRenderer.Init(particleCount);
            vectorFieldGenerator.Init();
            vectorField = vectorFieldGenerator.CreateVectorField(vectorFieldSize, in settings.simulationBound);
        }

        private int GetMaxCollisionCount()
        {
            // each particle can collid with every other particle n^2
            // if i hit j, we skip j hit i
            // n(n+1)/2 
            // we remove self particle hit
            return (particleCount * (particleCount - 1)) / 2;
        }

        void Update()
        {
            fogoRenderer.Render(in fireParticles, in settings);
            DrawDebugBounds();
            DrawVectorField();
            calentador.DrawDebug(in simData, in settings);
        }

        private void FixedUpdate()
        {
            simData = new SimulationData()
            {
                time = (Time.fixedDeltaTime * simulationSpeed),
                dt = (Time.fixedDeltaTime * simulationSpeed) / substeps,
                pos = transform.position,
            };

            vectorFieldGenerator.UpdateVectorField(ref vectorField, in settings.simulationBound);

            for (int i = 0; i < substeps; i++)
            {
                UpdateSimulation(in simData);
            }
        }

        void UpdateSimulation(in SimulationData simulationData)
        {
            calentador.HeatParticles(in simulationData, ref fireParticles, settings);

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
                    //hashingGrid[x, y].Clear();
                    var list = nativeHashingGrid[x, y];
                    list.Clear();
                    nativeHashingGrid[x, y] = list;
                }
            }

            for (int i = 0; i < fireParticles.Length; i++)
            {
                int2 hash = OFogoHelper.HashPosition(fireParticles[i].position, in settings.simulationBound, settings.hashingGridLength);

                //hashingGrid[hash.x, hash.y].Add(i);
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
                subSteps = substeps
            }.Run();
        }


        private void DrawDebugBounds()
        {
            float3 min = transform.position + settings.simulationBound.min;
            float3 max = transform.position + settings.simulationBound.max;
            float3 bottomLeft = new float3(min.x, min.y, 0f);
            float3 bottomRight = new float3(max.x, min.y, 0f);
            float3 topLeft = new float3(min.x, max.y, 0f);
            float3 topRight = new float3(max.x, max.y, 0f);
            Debug.DrawLine(bottomLeft, topLeft, Color.white);
            Debug.DrawLine(topLeft, topRight, Color.white);
            Debug.DrawLine(topRight, bottomRight, Color.white);
            Debug.DrawLine(bottomRight, bottomLeft, Color.white);

            float2 invLength = 1f / (float2)settings.hashingGridLength;
            for (int i = 0; i < settings.hashingGridLength.x; i++)
            {
                float yRatio = i * invLength.y;
                float y = math.lerp(min.y, max.y, yRatio);
                float3 start = new float3(min.x, y, min.z);
                float3 end = new float3(max.x, y, max.z);
                Debug.DrawLine(start, end, Color.cyan * 0.25f);
            }

            for (int i = 0; i < settings.hashingGridLength.y; i++)
            {
                float xRatio = i * invLength.x;
                float x = math.lerp(min.x, max.x, xRatio);
                float3 start = new float3(x, min.y, min.z);
                float3 end = new float3(x, max.y, max.z);
                Debug.DrawLine(start, end, Color.cyan * 0.25f);
            }
        }

        private void DrawVectorField()
        {
            float3 min = transform.position + settings.simulationBound.min;
            float3 max = transform.position + settings.simulationBound.max;

            float2 invSize = 1f / (float2)vectorField.Size;
            for (int x = 0; x < vectorField.Size.x; x++)
            {
                for (int y = 0; y < vectorField.Size.y; y++)
                {
                    float2 pos = new float2(x, y) * invSize;
                    float3 t = new float3(pos, 0);
                    float3 gridCenter = math.lerp(min, max, t);
                    float3 force = vectorField[x, y];
                    Debug.DrawRay(gridCenter, force * debugRayDist, Color.white);
                }
            }
        }
        private void OnDestroy()
        {
            vectorFieldGenerator.Dispose();
            fogoRenderer.Dispose();

            if (fireParticles.IsCreated)
            {
                fireParticles.Dispose();
                fireParticleCollisionPair.Dispose();

                for (int x = 0; x < settings.hashingGridLength.x; x++)
                {
                    for (int y = 0; y < settings.hashingGridLength.y; y++)
                    {
                        //hashingGrid[x, y].Dispose();
                        nativeHashingGrid[x, y].Dispose();
                    }
                }
                nativeHashingGrid.Dispose();
            }
        }
    }
}