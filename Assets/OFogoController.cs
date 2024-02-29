using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class OFogoController : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] OFogoSimulator fogoSimulator;
        [SerializeField] VectorFieldParticleSimulation vectorFieldSimulator;
        [SerializeField] Calentador calentador;
        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] VectorFieldRenderer vectorFieldRenderer;

        [Header("Simulation")]
        [SerializeField] int numberThreadJob = 16;
        [SerializeField] int2 vectorFieldSize = 35;
        [SerializeField] float simulationSpeed = 1;
        [SerializeField] int substeps = 4;
        [SerializeField] bool useFireSimulation;

        [Header("Debug")]
        [SerializeField] bool drawVectorFieldDebug;
        [SerializeField] bool drawBoundsDebug;
        [SerializeField] bool drawCalentadorDebug;
        [SerializeField] float debugRayDist = 5;
        [SerializeField] NativeLeakDetectionMode nativeLeakDetectionMode;

        private void Start()
        {
            fogoSimulator.Init();
            fogoRenderer.Init(fogoSimulator.particleCount);
            vectorFieldSimulator.Init(fogoSimulator.particleCount);
            fogoSimulator.vectorField = vectorFieldGenerator.CreateVectorField(vectorFieldSize, in fogoSimulator.settings.simulationBound);

            vectorFieldRenderer.Init(fogoSimulator.vectorField);
        }

        private void Update()
        {
            //stupid hack
            NativeLeakDetection.Mode = nativeLeakDetectionMode;

            fogoRenderer.Render(in fogoSimulator.fireParticles, in fogoSimulator.settings);
            vectorFieldRenderer.Render(in fogoSimulator.vectorField, in fogoSimulator.settings);
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

                calentador?.HeatParticles(in simData, ref fogoSimulator.fireParticles, fogoSimulator.settings);
                vectorFieldGenerator.UpdateVectorField(ref fogoSimulator.vectorField, in fogoSimulator.settings.simulationBound);

                if(useFireSimulation)
                {
                    fogoSimulator.TickSimulation(simData);
                }
                else
                {
                    fogoSimulator.FillHashGrid();
                    vectorFieldSimulator.TickSimulation(simData, fogoSimulator.fireParticles, fogoSimulator.vectorField, fogoSimulator.nativeHashingGrid, fogoSimulator.settings);
                }
            }
        }

        public NativeGrid<float3> VectorField { 
            get => fogoSimulator.vectorField; 
            set { fogoSimulator.vectorField = value; }}

        private void OnDestroy()
        {
            fogoSimulator.Dispose();
            vectorFieldGenerator.Dispose();
            fogoRenderer.Dispose();
            vectorFieldRenderer.Dispose();
            vectorFieldSimulator.Dispose();
        }

        private void DrawDebugBounds()
        {
            float3 min = transform.position + fogoSimulator.settings.simulationBound.min;
            float3 max = transform.position + fogoSimulator.settings.simulationBound.max;
            float3 bottomLeft = new float3(min.x, min.y, 0f);
            float3 bottomRight = new float3(max.x, min.y, 0f);
            float3 topLeft = new float3(min.x, max.y, 0f);
            float3 topRight = new float3(max.x, max.y, 0f);
            Debug.DrawLine(bottomLeft, topLeft, Color.white);
            Debug.DrawLine(topLeft, topRight, Color.white);
            Debug.DrawLine(topRight, bottomRight, Color.white);
            Debug.DrawLine(bottomRight, bottomLeft, Color.white);

            float2 invLength = 1f / (float2)fogoSimulator.settings.hashingGridLength;
            for (int i = 0; i < fogoSimulator.settings.hashingGridLength.x; i++)
            {
                float yRatio = i * invLength.y;
                float y = math.lerp(min.y, max.y, yRatio);
                float3 start = new float3(min.x, y, min.z);
                float3 end = new float3(max.x, y, max.z);
                Debug.DrawLine(start, end, Color.cyan * 0.25f);
            }

            for (int i = 0; i < fogoSimulator.settings.hashingGridLength.y; i++)
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
            float3 min = transform.position + fogoSimulator.settings.simulationBound.min;
            float3 max = transform.position + fogoSimulator.settings.simulationBound.max;

            float2 invSize = 1f / (float2)fogoSimulator.vectorField.Size;
            for (int x = 0; x < fogoSimulator.vectorField.Size.x; x++)
            {
                for (int y = 0; y < fogoSimulator.vectorField.Size.y; y++)
                {
                    float2 pos = new float2(x + 0.5f, y + 0.5f) * invSize;
                    float3 t = new float3(pos, 0);
                    float3 gridCenter = math.lerp(min, max, t);
                    float3 force = fogoSimulator.vectorField[x, y];
                    Debug.DrawRay(gridCenter, force * debugRayDist, Color.white);
                }
            }

        }
    }
}