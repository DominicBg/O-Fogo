using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class DrawingVectorField : SimpleVectorFieldGenerator
    {
        bool requestWipe;
        bool requestSmooth;
        bool isStrokeFinised;

        NativeList<float3> stroke;

        [SerializeField] Camera mainCam;
        [SerializeField] float strength = 0.1f;
        [SerializeField] float sampleInterval = 0.1f;
        float currentSampleInterval;

        NativeGrid<float3> tempVectorField;
        public NativeGrid<float3> vectorFieldCopy; //nasty

        public override void Init()
        {
            base.Init();
            stroke = new NativeList<float3>(100, Allocator.Persistent);
        }

        public override void Dispose()
        {
            base.Dispose();
            stroke.Dispose();
        }

        public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
        {
            vectorFieldCopy = vectorField;
            if (tempVectorField.IsCreated)
            {
                for (int x = 0; x < vectorField.Size.x; x++)
                {
                    for (int y = 0; y < vectorField.Size.y; y++)
                    {
                        vectorField[x, y] = tempVectorField[x, y];
                    }
                }
                tempVectorField.Dispose();
            }

            if (stroke.Length > 0 && isStrokeFinised)
            {
                for (int i = 1; i < stroke.Length; i++)
                {
                    int2 hashPos = FogoSimulator.HashPosition(stroke[i], bounds, vectorField.Size);
                    float3 delta = stroke[i] - stroke[i - 1];
                    vectorField[hashPos] += delta * strength;
                }

                stroke.Clear();
            }

            if (requestWipe)
            {
                for (int x = 0; x < vectorField.Size.x; x++)
                {
                    for (int y = 0; y < vectorField.Size.y; y++)
                    {
                        vectorField[x, y] = 0;
                    }
                }
                requestWipe = false;
            }

            if (requestSmooth)
            {
                for (int x = 0; x < vectorField.Size.x; x++)
                {
                    for (int y = 0; y < vectorField.Size.y; y++)
                    {
                        float3 sum = 0;
                        int sumCount = 0;
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            for (int yy = -1; yy <= 1; yy++)
                            {
                                int2 pos = new int2(x + xx, y + yy);

                                if (vectorField.InBound(pos))
                                {
                                    sum += vectorField[pos];
                                    sumCount++;
                                }
                            }
                        }
                        vectorField[x, y] = sum / sumCount;
                    }
                }
                requestSmooth = false;
            }
        }

        private void Update()
        {
            if(!stroke.IsCreated)
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                isStrokeFinised = false;
                stroke.Clear();
            }
            else if (Input.GetMouseButton(0))
            {
                currentSampleInterval += Time.deltaTime;
                if (currentSampleInterval > sampleInterval)
                {
                    currentSampleInterval -= sampleInterval;
                    Ray ray = mainCam.ScreenPointToRay(new float3(Input.mousePosition.x, Input.mousePosition.y, mainCam.nearClipPlane));

                    Plane plane = new Plane(Vector3.forward, 0);
                    plane.Raycast(ray, out float hitDist);

                    float3 mousePos = ray.origin + ray.direction * hitDist;

                    stroke.Add(mousePos);
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isStrokeFinised = true;
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                requestWipe = true;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                requestSmooth = true;
            }
            DebugStroke();
        }

        void DebugStroke()
        {
            for (int i = 1; i < stroke.Length; i++)
            {
                Debug.DrawLine(stroke[i], stroke[i - 1], Color.white);
            }
        }

        public void ImposeVectorField(NativeGrid<float3> tempVectorField)
        {
            this.tempVectorField = tempVectorField;
        }
    }
}