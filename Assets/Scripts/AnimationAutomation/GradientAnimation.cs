using UnityEngine;

namespace OFogo
{
    public class GradientAnimation : AnimationAutomation
    {
        [SerializeField] MagicController magicController;
        [SerializeField] int gradientIndexA;
        [SerializeField] int gradientIndexB;


        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float time, float timeRatio)
        {
            magicController.LerpGradient(gradientIndexA, gradientIndexB, timeRatio);
        }
    }
}