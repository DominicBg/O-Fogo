using UnityEngine;
namespace OFogo
{
    public class SimulatorBlendAnimation : AnimationAutomation
    {
        [SerializeField] SimulatorBlend simulationBlend;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            OFogoController.Instance.SetSimulator(simulationBlend);
        }

        public override void UpdateAnimation(float timeRatio)
        {
            simulationBlend.ratio = timeRatio;
        }
    }
}