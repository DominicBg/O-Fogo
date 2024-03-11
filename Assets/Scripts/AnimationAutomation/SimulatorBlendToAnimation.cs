using UnityEngine;

namespace OFogo
{
    public class SimulatorBlendToAnimation : AnimationAutomation
    {
        [SerializeField] FireParticleSimulator simulatorBlendTo;

        SimulatorBlend simulatorBlend;

        // Start is called before the first frame update
        void Awake()
        {
            var blendToAnim = new GameObject("SimulatorBlendToAnimation");
            blendToAnim.transform.SetParent(transform);
            simulatorBlend = blendToAnim.AddComponent<SimulatorBlend>();
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            simulatorBlend.fireParticleSimulatorA = OFogoController.Instance.GetCurrentSimulator();
            simulatorBlend.fireParticleSimulatorB = simulatorBlendTo;

            OFogoController.Instance.SetSimulator(simulatorBlend);
        }

        public override void UpdateAnimation(float timeRatio)
        {
            simulatorBlend.ratio = timeRatio;
        }

    }
}