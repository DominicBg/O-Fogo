using UnityEngine;

namespace OFogo
{
    public class RendererAlphaBlendAnimation : AnimationAutomation
    {
        [SerializeField] AlphaRenderer alphaRendererA;
        [SerializeField] AlphaRenderer alphaRendererB;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float timeRatio)
        {
            alphaRendererA.alpha = 1f - timeRatio;
            alphaRendererB.alpha = timeRatio;
        }
    }
}