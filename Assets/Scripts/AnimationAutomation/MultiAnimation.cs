using Unity.Mathematics;
using UnityEngine;

namespace OFogo
{ 
    public class MultiAnimation : AnimationAutomation
    {
        [SerializeField] AnimationAutomation[] animations;

        private void OnValidate()
        {
            for (int i = 0; i < animations.Length; i++)
            {
                duration = math.max(duration, animations[i].duration);
            }
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float timeRatio)
        {
            for (int i = 0; i < animations.Length; i++)
            {
                animations[i].UpdateAnimation(timeRatio);
            }
        }
    }
}