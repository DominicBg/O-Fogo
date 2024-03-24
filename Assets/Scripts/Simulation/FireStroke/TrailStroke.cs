using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class TrailStroke : PointListStroke
    {
        [SerializeField] float samplePointsInterval = 0.2f;
        [SerializeField] int maxPointCount = 100;
        [SerializeField] bool drawDebug;

        protected NativeCircularBuffer<float3> circularPointList;
        float currentSamplePointsDuration = 0;

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

        public void ClearPoints()
        {
            circularPointList.Clear();
        }

        private void Update()
        {
            currentSamplePointsDuration += Time.deltaTime;
            if (currentSamplePointsDuration > samplePointsInterval)
            {
                currentSamplePointsDuration -= samplePointsInterval;

                if(currentSamplePointsDuration > samplePointsInterval)
                {
                    currentSamplePointsDuration = 0;
                }
                circularPointList.Add(transform.position);
            }
            CopyPointsToStroke(circularPointList);

            if(drawDebug)
            {
                DebugDraw();
            }
        }

        void DebugDraw()
        {
            for (int i = 1; i < circularPointList.Length; i++)
            {
                Debug.DrawLine(circularPointList[i - 1], circularPointList[i]);
            }
        }
    }
}