using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class OFogoControllerDebugger : MonoBehaviour
    {
        [SerializeField] OFogoController controller;
        [SerializeField] bool drawVectorFieldDebug;
        [SerializeField] bool drawBoundsDebug;
        [SerializeField] bool drawCalentadorDebug;
        [SerializeField] float debugRayDist = 5;
        [SerializeField] NativeLeakDetectionMode nativeLeakDetectionMode;

        // Update is called once per frame
        private void Update()
        {      
            //stupid hack
            NativeLeakDetection.Mode = nativeLeakDetectionMode;
            DrawDebugBounds(controller.settings);
            DrawVectorField(controller.vectorField, controller.settings);

        }
        private void DrawDebugBounds(in SimulationSettings settings)
        {
            float3 min = controller.transform.position + settings.simulationBound.min;
            float3 max = controller.transform.position + settings.simulationBound.max;
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

        private void DrawVectorField(NativeGrid<float3> vectorField, in SimulationSettings settings)
        {
            float3 min = controller.transform.position + settings.simulationBound.min;
            float3 max = controller.transform.position + settings.simulationBound.max;

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