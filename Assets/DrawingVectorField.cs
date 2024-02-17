using OFogo;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class DrawingVectorField : VectorFieldGenerator
{
    bool requestWipe;
    bool isStrokeFinised;

    List<float3> stroke = new List<float3>();
    //NativeList<float3> stroke;

    [SerializeField] Camera mainCam;
    [SerializeField] float strength = 0.1f;
    [SerializeField] float sampleInterval = 0.1f;
    float currentSampleInterval;

    public override void Init()
    {
        base.Init();
        //stroke = new NativeList<float3>(100, Allocator.Persistent);
    }

    public override void Dispose()
    {
        base.Dispose();
        //stroke.Dispose();
    }

    public override void UpdateVectorField(ref NativeGrid<float3> vectorField, in Bounds bounds)
    {
        //if(stroke.Length > 0 && isStrokeFinised)
        if (stroke.Count > 0 && isStrokeFinised)
        {
            //for (int i = 1; i < stroke.Length; i++)
            for (int i = 1; i < stroke.Count; i++)
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
    }

    private void Update()
    {
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

                //float3 mousePos = mainCam.ScreenToWorldPoint(new float3(Input.mousePosition.x, Input.mousePosition.y, mainCam.nearClipPlane));
                Debug.Log(Input.mousePosition + " " + mousePos);
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

        DebugStroke();
    }

    void DebugStroke()
    {
        for (int i = 1; i < stroke.Count; i++)
        //for (int i = 1; i < stroke.Length; i++)
        {
            Debug.DrawLine(stroke[i], stroke[i - 1], Color.white);
        }
    }
}
