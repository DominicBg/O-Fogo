using UnityEngine;

namespace OFogo
{
    public class VectorFieldBlendToAnimation : AnimationAutomation
    {
        [SerializeField] VectorFieldGenerator vectorFieldBlendTo;

        BlendVectorField vectorFieldBlend;

        void Awake()
        {
            var blendToAnim = new GameObject("VectorFieldBlendToAnimation");
            blendToAnim.transform.SetParent(transform);
            vectorFieldBlend = blendToAnim.AddComponent<BlendVectorField>();
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            vectorFieldBlend.vectorFieldGeneratorA = OFogoController.Instance.GetCurrentVectorFieldGenerator();
            vectorFieldBlend.vectorFieldGeneratorB = vectorFieldBlendTo;

            OFogoController.Instance.SetVectorFieldGenerator(vectorFieldBlend);
        }

        public override void UpdateAnimation(float timeRatio)
        {
            vectorFieldBlend.ratio = timeRatio;
        }
    }

}