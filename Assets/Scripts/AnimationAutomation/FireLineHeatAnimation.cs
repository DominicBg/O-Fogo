using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{
    public class FireLineHeatAnimation : AnimationAutomation
    {
        [SerializeField] FireStrokeSimulator fireStrokeSimulator;
        [SerializeField] float minHeatHeight;
        [SerializeField] float maxHeatHeight;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float timeRatio)
        {
            fireStrokeSimulator.burnHeight = math.lerp(minHeatHeight, maxHeatHeight, timeRatio);
        }
    }
}