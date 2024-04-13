using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
namespace OFogo
{
    public class SerpenteDiFuoco : PointListStroke
    {
        public float3 startPos = 0;
        public float radius = 2;
        public float rotationSpeed = 1;
        public float heightSinAmplitude = 1;
        public float heightSinFrequency = 1;
        public float heightSinOffset = 0;

        [SerializeField] int subStep = 4;
        [SerializeField] int maxPointCount = 100;
        protected NativeCircularBuffer<float3> circularPointList;

        void FixedUpdate()
        {
            float subFixedDeltaTime = Time.fixedDeltaTime / subStep;
            for (int i = 0; i < subStep; i++)
            {
                float time = Time.fixedTime + i * subFixedDeltaTime;
                float3 samplePos = startPos + CalculatePos(heightSinAmplitude, heightSinFrequency, heightSinOffset, radius, rotationSpeed, time);
                circularPointList.Add(samplePos);
            }
            CopyPointsToStroke(circularPointList);
        }

        static float3 CalculatePos(float a, float f, float p, float r, float s, float t)
        {
            float3 y = math.up() * a * math.sin(t * f * math.PI * 2f + p);
            float3 xz = math.mul(float3x3.RotateZ(math.radians(s * t)), math.up()) * r;
            return y + xz;
        }

        public void ClearPoints()
        {
            circularPointList.Clear();
        }
        protected override void Start()
        {
            base.Start();
            circularPointList = new NativeCircularBuffer<float3>(maxPointCount, Allocator.Persistent);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            circularPointList.Dispose();
        }
    }
}