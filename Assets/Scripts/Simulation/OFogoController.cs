using System.Collections.Generic;
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

        public NativeGrid<float3> vectorField;
        public NativeArray<FireParticle> fireParticles;
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

        private HashSet<FireParticleSimulator> simulatorToDispose = new HashSet<FireParticleSimulator>();
        private HashSet<VectorFieldGenerator> vectorFieldGeneratorToDispose = new HashSet<VectorFieldGenerator>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            fireParticles = new NativeArray<FireParticle>(settings.particleCount, Allocator.Persistent);
            nativeHashingGrid = new NativeGrid<UnsafeList<int>>(CalculateNativeHashingGridSize(in settings), Allocator.Persistent);
            Debug.Log(nativeHashingGrid.Size);
            for (int x = 0; x < nativeHashingGrid.Size.x; x++)
            {
                for (int y = 0; y < nativeHashingGrid.Size.y; y++)
                {
                    nativeHashingGrid[x, y] = new UnsafeList<int>(32, Allocator.Persistent);
                }
            }
            SpawnParticles();

            fogoRenderer.Init(settings.particleCount);

            vectorField = VectorFieldGenerator.CreateVectorField(in settings);


            SetSimulator(simulator);
            SetVectorFieldGenerator(vectorFieldGenerator);

            vectorFieldRenderer.Init(vectorField);
        }

        static int2 CalculateNativeHashingGridSize(in SimulationSettings settings)
        {
            float2 size = ((float3)settings.simulationBound.size).xy;
            return (int2)math.ceil(size / settings.maxParticleSize);
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
            }
        }

        public void SetSimulator(FireParticleSimulator simulator)
        {
            if (simulator.TryInit(in settings))
            {
                simulatorToDispose.Add(simulator);
            }

            this.simulator = simulator;
        }
        public FireParticleSimulator GetCurrentSimulator() => simulator;

        public void SetVectorFieldGenerator(VectorFieldGenerator generator)
        {
            if (generator.TryInit(in settings))
            {
                vectorFieldGeneratorToDispose.Add(generator);
            }

            vectorFieldGenerator = generator;
        }
        public VectorFieldGenerator GetCurrentVectorFieldGenerator() => vectorFieldGenerator;



        public void UpdateSimulation(FireParticleSimulator simulator, in SimulationData simData, NativeArray<FireParticle> fireParticles)
        {
#if UNITY_EDITOR
            //for drag n drop support
            if (simulator.TryInit(in settings))
            {
                simulatorToDispose.Add(simulator);
            }
#endif

            if (!simulator.IsHandlingParticleHeating())
            {
                calentador?.HeatParticles(in simData, ref fireParticles, settings);
            }

            if (simulator.NeedsVectorField())
            {
#if UNITY_EDITOR
                //for drag n drop support
                if (vectorFieldGenerator.TryInit(in settings))
                {
                    vectorFieldGeneratorToDispose.Add(vectorFieldGenerator);
                }
#endif
                vectorFieldGenerator.UpdateVectorField(in simData, ref vectorField, in settings);
            }

            simulator.UpdateSimulation(in simData, ref fireParticles, vectorField, settings);

            if (simulator.CanResolveCollision())
            {
                FillHashGrid();
                simulator.ResolveCollision(simData, ref fireParticles, vectorField, nativeHashingGrid, settings);
            }
        }

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
            foreach (var simulator in simulatorToDispose)
            {
                simulator.Dispose();
            }
            foreach (var generator in vectorFieldGeneratorToDispose)
            {
                generator.Dispose();
            }
            vectorFieldGenerator.Dispose();
            fogoRenderer.Dispose();
            vectorFieldRenderer.Dispose();

            fireParticles.Dispose();
            for (int x = 0; x < nativeHashingGrid.Size.x; x++)
            {
                for (int y = 0; y < nativeHashingGrid.Size.y; y++)
                {
                    nativeHashingGrid[x, y].Dispose();
                }
            }
            nativeHashingGrid.Dispose();
            vectorField.Dispose();
        }
    }
}