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
        [SerializeField] float initialSpacing = 0.5f; //move initiation in interface
        [SerializeField] int particleCount;
        [SerializeField] uint seed = 43243215;
        [SerializeField] int substeps = 4;
        [SerializeField] float simulationSpeed = 1;
        [SerializeField] bool parallelCollision;

        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] int2 vectorFieldSize = 35;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] SimulationSettings settings;

        Unity.Mathematics.Random rng;
        public NativeGrid<float3> vectorField;

        public struct FireParticleCollision
        {
            public int indexA;
            public int indexB;
            public float distSq;

            public FireParticleCollision(int indexA, int indexB, float distSq)
            {
                this.indexA = indexA;
                this.indexB = indexB;
                this.distSq = distSq;
            }
        }

        NativeArray<FireParticle> fireParticles;
        NativeList<FireParticleCollision> fireParticleCollisionPair;

        NativeList<int>[,] hashingGrid;

        NativeGrid<UnsafeList<int>> nativeHashingGrid;

        void Start()
        {
            rng = Unity.Mathematics.Random.CreateFromIndex(seed);

            fireParticles = new NativeArray<FireParticle>(particleCount, Allocator.Persistent);
            fireParticleCollisionPair = new NativeList<FireParticleCollision>(GetMaxCollisionCount(), Allocator.Persistent);
            hashingGrid = new NativeList<int>[settings.hashingGridLength.x, settings.hashingGridLength.y];
            nativeHashingGrid = new NativeGrid<UnsafeList<int>>(settings.hashingGridLength, Allocator.Persistent);

            for (int x = 0; x < settings.hashingGridLength.x; x++)
            {
                for (int y = 0; y < settings.hashingGridLength.y; y++)
                {
                    hashingGrid[x, y] = new NativeList<int>(Allocator.Persistent);
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
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime / substeps;
            dt *= simulationSpeed;
            vectorFieldGenerator.UpdateVectorField(ref vectorField, in settings.simulationBound);
            for (int i = 0; i < substeps; i++)
            {
                UpdateSimulation(dt);
            }
        }

        [BurstCompile]
        public struct UpdateSimulationJob : IJobParallelFor
        {
            public float dt;
            public float time;
            public NativeArray<FireParticle> fireParticles;
            public SimulationSettings settings;
            public float3 simPos;

            [ReadOnly]
            public NativeGrid<float3> vectorField;

            public void Execute(int index)
            {
                float minSimulationY = simPos.y + settings.simulationBound.min.y;

                FireParticle fireParticle = fireParticles[index];

                fireParticle.temperature -= settings.temperatureDropPerSec * dt;
                fireParticle.temperature = math.max(fireParticle.temperature, 0f);

                float heightSmoothStep = 1f - math.smoothstep(minSimulationY, minSimulationY + settings.heatAtBottomRange, fireParticle.position.y);
                
                if (settings.heatAtBottomNoiseRatio > 0)
                {
                    float heatNoise = noise.cnoise(new float2(time, fireParticle.position.x * settings.heatAtBottomNoiseSize));
                    heightSmoothStep *= math.lerp(1, heatNoise, settings.heatAtBottomNoiseRatio);
                }

                float heat = heightSmoothStep * settings.heatAtBottomPerSec;
                fireParticle.temperature += heat * dt;
                fireParticle.temperature = math.clamp(fireParticle.temperature, 0, settings.maxTemperature);

                fireParticle.radius = math.lerp(settings.minParticleSize, settings.maxParticleSize, fireParticle.temperature / settings.maxTemperature);

                float3 heatTurbulence = vectorField[HashPosition(fireParticle.position, settings.simulationBound, vectorField.Size)];
                float3 acceleration = 0;

                if (settings.useVectorFieldAsGravity)
                {
                    acceleration += heatTurbulence * (settings.fireGravity + fireParticle.temperature * settings.temperatureUpwardForce);
                }
                else
                {
                    acceleration += math.up() * (settings.fireGravity + fireParticle.temperature * settings.temperatureUpwardForce);
                    acceleration += heatTurbulence;
                }


                switch (settings.integrationType)
                {
                    case IntegrationType.Euler:

                        fireParticle.velocity += acceleration * dt;
                        if (math.lengthsq(fireParticle.velocity) > Pow2(settings.maxSpeed))
                        {
                            fireParticle.velocity = math.normalize(fireParticle.velocity) * settings.maxSpeed;
                        }
                        fireParticle.position += fireParticle.velocity * dt;

                        break;
                    case IntegrationType.Verlet:

                        float3 velocity = fireParticle.position - fireParticle.prevPosition;
                        if (math.lengthsq(velocity) > Pow2(settings.maxSpeed))
                        {
                            velocity = math.normalize(velocity) * settings.maxSpeed;
                        }
                        fireParticle.prevPosition = fireParticle.position;
                        fireParticle.position += velocity + acceleration * dt * dt;
                        break;
                }

                //clamp
                ApplyConstraintBounce(ref fireParticle, settings);
                fireParticles[index] = fireParticle;
            }
        }


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
                int2 hash = HashPosition(fireParticles[index].position, in settings.simulationBound, settings.hashingGridLength);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        CheckCollisionPair(hash, index, x, y, fireParticles, nativeHashingGrid, settings, ref collisionBuffer);
                    }
                }
                fireParticleCollisionPair.AddRangeNoResize(collisionBuffer);
                collisionBuffer.Dispose();
            }
        }

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
                    int2 hash = HashPosition(fireParticles[i].position, in settings.simulationBound, settings.hashingGridLength);

                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            CheckCollisionPair(hash, i, x, y, fireParticles, nativeHashingGrid, settings, ref collisionBuffer);
                        }
                    }
                }
                fireParticleCollisionPair.AddRange(collisionBuffer);
                collisionBuffer.Dispose();
            }
        }

        [BurstCompile]
        public struct ResolveCollisionJob : IJob
        {
            public NativeArray<FireParticle> fireParticles;
            public NativeList<FireParticleCollision> fireParticleCollisionPair;
            public SimulationSettings settings;
            public NativeReference<Unity.Mathematics.Random> rngRef;

            public void Execute()
            {
                var rng = rngRef.Value;

                //resolve collision
                for (int i = 0; i < fireParticleCollisionPair.Length; i++)
                {
                    FireParticleCollision pair = fireParticleCollisionPair[i];
                    FireParticle particleA = fireParticles[pair.indexA];
                    FireParticle particleB = fireParticles[pair.indexB];

                    float dist;
                    float3 dir;
                    if (math.lengthsq(pair.distSq) <= math.FLT_MIN_NORMAL)
                    {
                        dir = new float3(rng.NextFloat2Direction(), 0.0f);
                        dist = (particleA.radius + particleB.radius) * 0.1f;
                    }
                    else
                    {
                        dist = math.sqrt(pair.distSq);
                        float3 diff = particleA.position - particleB.position;
                        dir = diff / dist;
                    }

                    float penetration = (particleA.radius + particleB.radius) - dist;

                    float3 delta = 0.5f * dir * penetration;

                    particleA.position += delta * settings.resolutionStepRatio;
                    particleB.position -= delta * settings.resolutionStepRatio;

                    particleA.velocity += delta * settings.colisionVelocityResolution;
                    particleB.velocity -= delta * settings.colisionVelocityResolution;

                    ApplyConstraintBounce(ref particleA, settings);
                    ApplyConstraintBounce(ref particleB, settings);

                    fireParticles[pair.indexA] = particleA;
                    fireParticles[pair.indexB] = particleB;
                }

                rngRef.Value = rng;
            }
        }

        [BurstCompile]
        public struct ResolveTemperatureTransferJob : IJob
        {
            public NativeArray<FireParticle> fireParticles;
            public NativeList<FireParticleCollision> fireParticleCollisionPair;
            public SimulationSettings settings;
            public int subSteps;

            public void Execute()
            {
                for (int i = 0; i < fireParticleCollisionPair.Length; i++)
                {
                    FireParticleCollision pair = fireParticleCollisionPair[i];
                    FireParticle particleA = fireParticles[pair.indexA];
                    FireParticle particleB = fireParticles[pair.indexB];

                    float tempA = particleA.temperature;
                    float tempB = particleB.temperature;
                    float t = settings.heatTransferPercent / subSteps;
                    particleA.temperature = math.lerp(tempA, tempB, t);
                    particleB.temperature = math.lerp(tempB, tempA, t);

                    fireParticles[pair.indexA] = particleA;
                    fireParticles[pair.indexB] = particleB;
                }
            }
        }

        void UpdateSimulation(float dt)
        {
            new UpdateSimulationJob()
            {
                fireParticles = fireParticles,
                time = Time.time * settings.heatAtBottomNoiseSpeed,
                dt = dt,
                settings = settings,
                simPos = transform.position,
                vectorField = vectorField
            }.Schedule(fireParticles.Length, fireParticles.Length / 16).Complete();


            fireParticleCollisionPair.Clear();

            for (int x = 0; x < settings.hashingGridLength.x; x++)
            {
                for (int y = 0; y < settings.hashingGridLength.y; y++)
                {
                    hashingGrid[x, y].Clear();
                    var list = nativeHashingGrid[x, y];
                    list.Clear();
                    nativeHashingGrid[x, y] = list;
                }
            }
            for (int i = 0; i < fireParticles.Length; i++)
            {
                int2 hash = HashPosition(fireParticles[i].position, in settings.simulationBound, settings.hashingGridLength);

                hashingGrid[hash.x, hash.y].Add(i);
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

        public static void CheckCollisionPair(int2 hash, int i, int x, int y,
        in NativeArray<FireParticle> fireParticles, in NativeGrid<UnsafeList<int>> nativeHashingGrid,
        in SimulationSettings settings, ref NativeList<FireParticleCollision> collisionBuffer)
        {
            int2 pos = new int2(x + hash.x, y + hash.y);

            if (pos.x < 0 || pos.x >= settings.hashingGridLength.x || pos.y < 0 || pos.y >= settings.hashingGridLength.y)
            {
                return;
            }

            var gridList = nativeHashingGrid[pos.x, pos.y];

            for (int k = 0; k < gridList.Length; k++)
            {
                int j = gridList[k];
                //same particle or already processed the collision when i -> j,  don't need j -> i
                if (i >= j)
                {
                    continue;
                }

                float distSq = math.distancesq(fireParticles[i].position, fireParticles[j].position);
                float radiusSqSum = Pow2(fireParticles[i].radius + fireParticles[j].radius);

                //precompute penetration?
                if (distSq < radiusSqSum)
                {
                    collisionBuffer.Add(new FireParticleCollision(i, j, distSq));
                }
            }
        }

        public static int2 HashPosition(in float3 position, in Bounds bounds, int2 sizes)
        {
            float3 min = bounds.min;
            float3 max = bounds.max;
            float3 positionClamp = math.clamp(position, min, max);
            float3 t = math.unlerp(min, max, positionClamp);
            return math.clamp((int2)(t.xy * sizes), 0, sizes - 1);
        }

        public static int2 quantize(int2 v, int2 cellSize)
        {
            return new int2(math.floor(v / (float2)cellSize));
        }

        public static void ApplyConstraintBounce(ref FireParticle fireParticles, in SimulationSettings settings)
        {
            if (!settings.simulationBound.Contains(fireParticles.position))
            {
                bool3 outOfBounds = new bool3(
                    fireParticles.position.x < settings.simulationBound.min.x || fireParticles.position.x > settings.simulationBound.max.x,
                    fireParticles.position.y < settings.simulationBound.min.y || fireParticles.position.y < settings.simulationBound.max.y,
                    false
                );

                fireParticles.velocity = math.select(fireParticles.velocity, -fireParticles.velocity * settings.wallBounceIntensity, outOfBounds);
                fireParticles.position = settings.simulationBound.ClosestPoint(fireParticles.position);
            }
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

            float3 heatLeft = bottomLeft + math.up() * settings.heatAtBottomRange;
            float3 heatRight = bottomRight + math.up() * settings.heatAtBottomRange;
            Debug.DrawLine(heatLeft, heatRight, Color.red);
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
                    Debug.DrawRay(gridCenter, force * 0.2f, Color.white);
                }
            }
        }
        public static float Pow2(float x) => x * x;

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
                        hashingGrid[x, y].Dispose();
                        nativeHashingGrid[x, y].Dispose();
                    }
                }
                nativeHashingGrid.Dispose();
            }
        }
    }
}