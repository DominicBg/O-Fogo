using UnityEngine;

namespace OFogo
{
    public class VectorFieldBlendAnimation : AnimationAutomation
    {
        [SerializeField] BlendVectorField blendVector;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            OFogoController.Instance.SetVectorFieldGenerator(blendVector);
        }

        public override void UpdateAnimation(float timeRatio)
        {
            blendVector.t = timeRatio;
        }
    }
}