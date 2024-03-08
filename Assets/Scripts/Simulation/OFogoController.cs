using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class OFogoController : MonoBehaviour
    {
        public static OFogoController Instance;

        [Header("Components")]
        [SerializeField] FireParticleSimulator simulator;

        [SerializeField] Calentador calentador;
        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] VectorFieldRenderer vectorFieldRenderer;

        [Header("Simulation")]
        public SimulationSettings settings;
        [SerializeField] float initialSpacing = 0.5f;
        [SerializeField] int numberThreadJob = 16;
        [SerializeField] float simulationSpeed = 1;
        [SerializeField] int substeps = 4;

        //public EFireSimulatorType fireSimulatorTypeA;
        //public EFireSimulatorType fireSimulatorTypeB;
        //[Range(0, 1)]
        //public float fireSimulatorLerpRatio = 0;
        //public bool isFireSimulatorLerpAdditive;

        public NativeGrid<float3> vectorField;
        public NativeArray<FireParticle> fireParticles;
        //public NativeArray<FireParticle> fireParticlesB;
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            fireParticles = new NativeArray<FireParticle>(settings.particleCount, Allocator.Persistent);
            nativeHashingGrid = new NativeGrid<UnsafeList<int>>(settings.hashingGridLength, Allocator.Persistent);

            for (int x = 0; x < settings.hashingGridLength.x; x++)
            {
                for (int y = 0; y < settings.hashingGridLength.y; y++)
                {
                    nativeHashingGrid[x, y] = new UnsafeList<int>(32, Allocator.Persistent);
                }
            }
            SpawnParticles();

            fogoRenderer.Init(settings.particleCount);

            vectorField = VectorFieldGenerator.CreateVectorField(in settings);

            vectorFieldGenerator.TryInit(in settings);
            vectorFieldRenderer.Init(vectorField);
        }

        void SpawnParticles()
        {
            int particlePerCol = (int)math.sqrt(settings.particleCount);
            for (int i = 0; i < settings.particleCount; i++)
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

        private void Update()
        {
            if (fogoRenderer.alpha > 0)
            {
                fogoRenderer.Render(in fireParticles, in settings);
            }

            if (vectorFieldRenderer.alpha > 0)
            {
                vectorFieldRenderer.Render(in vectorField, in settings);
            }
            calentador?.DrawDebug(transform.position, in settings);

        }

        private void FixedUpdate()
        {
            JobUtility.numberOfThread = numberThreadJob;

            for (int i = 0; i < substeps; i++)
            {
                float dt = (Time.fixedDeltaTime * simulationSpeed) / substeps;
                SimulationData simData = new SimulationData()
                {
                    time = (Time.fixedTime * simulationSpeed) + dt * i,
                    dt = dt,
                    pos = transform.position,
                };

                UpdateSimulation(simulator, in simData, fireParticles);

                //IFireParticleSimulator simulatorB = GetSimulator(fireSimulatorTypeB);

                //if (fireSimulatorLerpRatio == 0)
                //{
                //    UpdateSimulation(simulatorA, in simData, fireParticles);
                //}
                //else if (fireSimulatorLerpRatio == 1)
                //{
                //    UpdateSimulation(simulatorB, in simData, fireParticles);
                //}
                //else
                //{
                //    UpdateLerpSimulation(simulatorA, simulatorB, simData, fireParticles, fireParticlesB, fireSimulatorLerpRatio);
                //}
            }
        }

        public void UpdateSimulation(FireParticleSimulator simulator, in SimulationData simData, NativeArray<FireParticle> fireParticles)
        {
            simulator.TryInit(in settings);

            if (!simulator.IsHandlingParticleHeating())
            {
                calentador?.HeatParticles(in simData, ref fireParticles, settings);
            }

            if (simulator.NeedsVectorField())
            {
                vectorFieldGenerator.UpdateVectorField(ref vectorField, in settings);
            }

            simulator.UpdateSimulation(in simData, ref fireParticles, vectorField, settings);

            if (simulator.CanResolveCollision())
            {
                FillHashGrid();
                simulator.ResolveCollision(simData, ref fireParticles, vectorField, nativeHashingGrid, settings);
            }
        }

        //NativeArray<FireParticle> UpdateLerpSimulation(IFireParticleSimulator simulatorA, IFireParticleSimulator simulatorB, in SimulationData simData, NativeArray<FireParticle> fireParticlesA, NativeArray<FireParticle> fireParticleBuffer, float t)
        //{
        //    UpdateSimulation(simulatorA, simData, fireParticlesA);
        //    UpdateSimulation(simulatorB, simData, fireParticleBuffer);

        //    new LerpParticleJobs(fireParticlesA, fireParticlesB, t, isFireSimulatorLerpAdditive).RunParralelAndProfile(fireParticlesA.Length);
        //    return fireParticlesA;
        //}

        //public struct LerpParticleJobs : IJobParallelFor
        //{
        //    NativeArray<FireParticle> fireParticlesA;
        //    NativeArray<FireParticle> fireParticlesB;
        //    public float t;
        //    public bool isFireSimulatorLerpAdditive;

        //    public LerpParticleJobs(NativeArray<FireParticle> fireParticlesA, NativeArray<FireParticle> fireParticlesB, float t, bool isFireSimulatorLerpAdditive)
        //    {
        //        this.fireParticlesA = fireParticlesA;
        //        this.fireParticlesB = fireParticlesB;
        //        this.isFireSimulatorLerpAdditive = isFireSimulatorLerpAdditive;
        //        this.t = t;
        //    }

        //    public void Execute(int index)
        //    {
        //        FireParticle fireParticle = FireParticle.Lerp(fireParticlesA[index], fireParticlesB[index], t);
        //        fireParticlesA[index] = fireParticle;

        //        //if not, it won't reset the position of the particles B, so it will do a drag effect
        //        if (!isFireSimulatorLerpAdditive)
        //        {
        //            fireParticlesB[index] = fireParticle;
        //        }
        //    }
        //}

        public void FillHashGrid()
        {
            new FillHashGridJob(fireParticles, nativeHashingGrid, settings).RunAndProfile();
        }

        public NativeGrid<float3> VectorField
        {
            get => vectorField;
            set { vectorField = value; }
        }

        private void OnDestroy()
        {
            simulator.Dispose();
            vectorFieldGenerator.Dispose();
            fogoRenderer.Dispose();
            vectorFieldRenderer.Dispose();

            fireParticles.Dispose();
            for (int x = 0; x < settings.hashingGridLength.x; x++)
            {
                for (int y = 0; y < settings.hashingGridLength.y; y++)
                {
                    nativeHashingGrid[x, y].Dispose();
                }
            }
            nativeHashingGrid.Dispose();
            vectorField.Dispose();
        }
    }
}