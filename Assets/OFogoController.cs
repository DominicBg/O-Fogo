using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class OFogoController : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] OFogoSimulator simulator;
        [SerializeField] Calentador calentador;
        [SerializeField] FogoRenderer fogoRenderer;
        [SerializeField] VectorFieldGenerator vectorFieldGenerator;
        [SerializeField] VectorFieldRenderer vectorFieldRenderer;

        [Header("Simulation")]
        [SerializeField] int2 vectorFieldSize = 35;
        [SerializeField] float simulationSpeed = 1;
        [SerializeField] int substeps = 4;

        [Header("Debug")]
        [SerializeField] bool drawVectorFieldDebug;
        [SerializeField] bool drawBoundsDebug;
        [SerializeField] bool drawCalentadorDebug;
        [SerializeField] float debugRayDist = 5;

        private void Start()
        {
            simulator.Init();
            fogoRenderer.Init(simulator.particleCount);

            simulator.vectorField = vectorFieldGenerator.CreateVectorField(vectorFieldSize, in simulator.settings.simulationBound);

            vectorFieldRenderer.Init(simulator.vectorField);
        }

        private void Update()
        {
            fogoRenderer.Render(in simulator.fireParticles, in simulator.settings);
            vectorFieldRenderer.Render(in simulator.vectorField, in simulator.settings);
            DrawDebugBounds();
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < substeps; i++)
            {
                float dt = (Time.fixedDeltaTime * simulationSpeed) / substeps;
                SimulationData simData = new SimulationData()
                {
                    time = (Time.fixedTime * simulationSpeed) + dt * i,
                    dt = dt,
                    pos = transform.position,
                };

                calentador?.HeatParticles(in simData, ref simulator.fireParticles, simulator.settings);
                vectorFieldGenerator.UpdateVectorField(ref simulator.vectorField, in simulator.settings.simulationBound);
                simulator.TickSimulation(simData);
            }
        }

        public NativeGrid<float3> VectorField { 
            get => simulator.vectorField; 
            set { simulator.vectorField = value; }}

        private void OnDestroy()
        {
            simulator.Dispose();
            vectorFieldGenerator.Dispose();
            fogoRenderer.Dispose();
            vectorFieldRenderer.Dispose();
        }

        private void DrawDebugBounds()
        {
            float3 min = transform.position + simulator.settings.simulationBound.min;
            float3 max = transform.position + simulator.settings.simulationBound.max;
            float3 bottomLeft = new float3(min.x, min.y, 0f);
            float3 bottomRight = new float3(max.x, min.y, 0f);
            float3 topLeft = new float3(min.x, max.y, 0f);
            float3 topRight = new float3(max.x, max.y, 0f);
            Debug.DrawLine(bottomLeft, topLeft, Color.white);
            Debug.DrawLine(topLeft, topRight, Color.white);
            Debug.DrawLine(topRight, bottomRight, Color.white);
            Debug.DrawLine(bottomRight, bottomLeft, Color.white);

            float2 invLength = 1f / (float2)simulator.settings.hashingGridLength;
            for (int i = 0; i < simulator.settings.hashingGridLength.x; i++)
            {
                float yRatio = i * invLength.y;
                float y = math.lerp(min.y, max.y, yRatio);
                float3 start = new float3(min.x, y, min.z);
                float3 end = new float3(max.x, y, max.z);
                Debug.DrawLine(start, end, Color.cyan * 0.25f);
            }

            for (int i = 0; i < simulator.settings.hashingGridLength.y; i++)
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
            float3 min = transform.position + simulator.settings.simulationBound.min;
            float3 max = transform.position + simulator.settings.simulationBound.max;

            float2 invSize = 1f / (float2)simulator.vectorField.Size;
            for (int x = 0; x < simulator.vectorField.Size.x; x++)
            {
                for (int y = 0; y < simulator.vectorField.Size.y; y++)
                {
                    float2 pos = new float2(x + 0.5f, y + 0.5f) * invSize;
                    float3 t = new float3(pos, 0);
                    float3 gridCenter = math.lerp(min, max, t);
                    float3 force = simulator.vectorField[x, y];
                    Debug.DrawRay(gridCenter, force * debugRayDist, Color.white);
                }
            }

        }
    }
}