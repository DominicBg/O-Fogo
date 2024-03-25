using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class OFogoController : MonoBehaviour
    {
        public static OFogoController Instance;
        public bool isSingleton;

        [Header("Components")]
        [SerializeField] FireParticleSimulator simulator;
        [SerializeField] Calentador calentador;
        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] VectorFieldRenderer vectorFieldRenderer;

        [Header("Simulation")]
        public SimulationSettings settings;
        public float simulationSpeed = 1;
        [SerializeField] int numberThreadJob = 16;
        [SerializeField] int substeps = 4;
        [SerializeField] int maxSimulationPerFrame = 1;

        public NativeGrid<float3> vectorField;
        public NativeArray<FireParticle> fireParticles;
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

        private int currentSimulationPerFrame = 0;
        private HashSet<FireParticleSimulator> simulatorToDispose = new HashSet<FireParticleSimulator>();
        private HashSet<VectorFieldGenerator> vectorFieldGeneratorToDispose = new HashSet<VectorFieldGenerator>();

        private void Awake()
        {
            if(isSingleton)
            {
                if(Instance != null)
                {
                    Debug.LogError(nameof(OFogoController) + " has multiple Singletons");
                }
                Instance = this;
            }
        }

        private void Start()
        {
            fireParticles = new NativeArray<FireParticle>(settings.particleCount, Allocator.Persistent);
            nativeHashingGrid = new NativeGrid<UnsafeList<int>>(CalculateNativeHashingGridSize(in settings), Allocator.Persistent);

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

            int maxRow = settings.particleCount / particlePerCol;
            for (int i = 0; i < settings.particleCount; i++)
            {
                int2 xy = new int2(i % particlePerCol, i / particlePerCol);
                float2 xyRatio = xy / new float2(particlePerCol, maxRow);

                float3 pos = math.lerp(settings.simulationBound.min, settings.simulationBound.max, new float3(xyRatio, 0));

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
            else if(fogoRenderer.HasStoppedRenderingThisFrame)
            {
                fogoRenderer.OnStopRendering(in fireParticles, in settings);
                fogoRenderer.alpha = 0;
                fogoRenderer.renderedLastFrame = false;
            }

            if (vectorFieldRenderer.alpha > 0)
            {
                vectorFieldRenderer.Render(in vectorField, in settings);
            }
            else if(vectorFieldRenderer.HasStoppedRenderingThisFrame)
            {
                vectorFieldRenderer.OnStopRendering(in vectorField, in settings);
                vectorFieldRenderer.alpha = 0;
                vectorFieldRenderer.renderedLastFrame = false;
            }

            calentador?.DrawDebug(transform.position, in settings);

            currentSimulationPerFrame = 0;
        }

        private void FixedUpdate()
        {
            JobUtility.numberOfThread = numberThreadJob;

            if(currentSimulationPerFrame > maxSimulationPerFrame)
            {
                return;
            }

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
            currentSimulationPerFrame++;
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

        public void SetCalentador(Calentador calentador)
        {
            this.calentador = calentador;
        }


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