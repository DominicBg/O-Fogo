using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class OFogoController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] OFogoSimulator fogoSimulator;
        [SerializeField] VectorFieldParticleSimulator vectorFieldSimulator;
        [SerializeField] FireStrokeSimulator fireStrokeSimulator;

        [SerializeField] Calentador calentador;
        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] VectorFieldRenderer vectorFieldRenderer;

        [Header("Simulation")]
        [SerializeField] public SimulationSettings settings;
        [SerializeField] float initialSpacing = 0.5f;
        [SerializeField] int numberThreadJob = 16;
        [SerializeField] int2 vectorFieldSize = 35;
        [SerializeField] float simulationSpeed = 1;
        [SerializeField] int substeps = 4;
        [SerializeField] EFireSimulatorType fireSimulatorType;
        public enum EFireSimulatorType { FOGO, VectorField, Stroke }

        //todo move in other component
        [Header("Debug")]
        [SerializeField] bool drawVectorFieldDebug;
        [SerializeField] bool drawBoundsDebug;
        [SerializeField] bool drawCalentadorDebug;
        [SerializeField] float debugRayDist = 5;
        [SerializeField] NativeLeakDetectionMode nativeLeakDetectionMode;

        public NativeGrid<float3> vectorField;
        public NativeArray<FireParticle> fireParticles;
        public NativeGrid<UnsafeList<int>> nativeHashingGrid;

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

            fogoSimulator.Init(in settings);
            fogoRenderer.Init(settings.particleCount);
            vectorFieldSimulator.Init(in settings);
            fireStrokeSimulator.Init(in settings);

            vectorField = vectorFieldGenerator.CreateVectorField(vectorFieldSize, in settings.simulationBound);
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
            //stupid hack
            NativeLeakDetection.Mode = nativeLeakDetectionMode;

            fogoRenderer.Render(in fireParticles, in settings);
            vectorFieldRenderer.Render(in vectorField, in settings);
            DrawDebugBounds();
            DrawVectorField();
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


                IFireParticleSimulator currentSimulator = null;
                switch (fireSimulatorType)
                {
                    case EFireSimulatorType.FOGO:
                        currentSimulator = fogoSimulator;
                        break;
                    case EFireSimulatorType.VectorField:
                        currentSimulator = vectorFieldSimulator;
                        break;
                    case EFireSimulatorType.Stroke:
                        currentSimulator = fireStrokeSimulator;
                        break;
                }

                if(!currentSimulator.IsHandlingParticleHeating())
                {
                    calentador?.HeatParticles(in simData, ref fireParticles, settings);
                }

                if(currentSimulator.NeedsVectorField())
                {
                    vectorFieldGenerator.UpdateVectorField(ref vectorField, in settings.simulationBound);
                }

                currentSimulator.UpdateSimulation(in simData, ref fireParticles, vectorField, settings);

                if (currentSimulator.CanResolveCollision())
                {
                    FillHashGrid();
                    currentSimulator.ResolveCollision(simData, ref fireParticles, vectorField, nativeHashingGrid, settings);
                }
            }
        }
        public void FillHashGrid()
        {
            new FillHashGridJob()
            {
                fireParticles = fireParticles,
                nativeHashingGrid = nativeHashingGrid,
                settings = settings
            }.Run();
        }

        public NativeGrid<float3> VectorField
        {
            get => vectorField;
            set { vectorField = value; }
        }

        private void OnDestroy()
        {
            fogoSimulator.Dispose();
            vectorFieldGenerator.Dispose();
            fogoRenderer.Dispose();
            vectorFieldRenderer.Dispose();
            vectorFieldSimulator.Dispose();
            fireStrokeSimulator.Dispose();

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
                    float2 pos = new float2(x + 0.5f, y + 0.5f) * invSize;
                    float3 t = new float3(pos, 0);
                    float3 gridCenter = math.lerp(min, max, t);
                    float3 force = vectorField[x, y];
                    Debug.DrawRay(gridCenter, force * debugRayDist, Color.white);
                }
            }

        }
    }
}