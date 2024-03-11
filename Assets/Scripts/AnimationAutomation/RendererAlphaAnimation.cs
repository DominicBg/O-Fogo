using UnityEngine;

namespace OFogo
{
    public class RendererAlphaAnimation : AnimationAutomation
    {
        [SerializeField] AlphaRenderer alphaRenderer;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float timeRatio)
        {
            alphaRenderer.alpha = timeRatio;
        }
    }
}