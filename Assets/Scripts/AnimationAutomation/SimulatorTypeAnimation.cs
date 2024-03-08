using UnityEngine;

namespace OFogo
{
    public class SimulatorTypeAnimation : AnimationAutomation
    {
        [SerializeField] OFogoController controller;
       // [SerializeField] EFireSimulatorType simulatorTypeA;
        //[SerializeField] EFireSimulatorType simulatorTypeB;

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
          //  controller.fireSimulatorTypeA = simulatorTypeA;
         //   controller.fireSimulatorTypeB = simulatorTypeB;
        }

        public override void UpdateAnimation(float timeRatio)
        {
         //   controller.fireSimulatorLerpRatio = timeRatio;
        }
    }
}