using UnityEngine;

namespace OFogo
{
    public class VectorFieldBlendAnimation : AnimationAutomation
    {
        [SerializeField] OFogoController controller;
        [SerializeField] BlendVectorField blendVector;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            blendVector.TryInit(controller.settings);
        }

        public override void UpdateAnimation(float timeRatio)
        {
            blendVector.t = timeRatio;
        }
    }
}