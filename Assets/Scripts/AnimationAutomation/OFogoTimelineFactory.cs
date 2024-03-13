using UnityEngine;
using UnityEngine.Rendering;

namespace OFogo.Animations
{
    public class OFogoTimelineFactory : MonoBehaviour, AnimationTimelineFactory
    {
        [Header("Simulator")]
        [SerializeField] FireStrokeSimulator logoSimulator;
        [SerializeField] OFogoSimulator ofogoSimulator;
        [SerializeField] SimulatorBlend solarPowerSimulator;

        [Header("VectorField")]
        [SerializeField] BlendVectorField fixedTurbulenceVectorFieldGenerator;
        [SerializeField] BlendVectorField radialTurbulenceVectorFieldGenerator;

        [Header("Renderers")]
        [SerializeField] FogoParticleRenderer fogoParticleRenderer;

        [Header("Gradient")]
        [SerializeField] VolumeProfile volumeProfile;
        [SerializeField] GPUColorGradientScriptable fireGradient;
        [SerializeField] GPUColorGradientScriptable blueGradient;

        AnimationTimelineController animationTimelineController;

        void Start()
        {
            animationTimelineController = new AnimationTimelineController(this);
            animationTimelineController.Start(cycleAnimations: true);
        }

        void Update()
        {
            animationTimelineController.Update(Time.deltaTime);
        }

        public void CreateAnimationTimeLine(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));
            timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
            timeline.AddOnStart(() => fogoParticleRenderer.alpha = 0);

            timeline.Add(
               new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
           ).SetDuration(0);

            timeline.Add(
                new RendererAlpha(fogoParticleRenderer, 0, 1)
            ).SetDuration(2);

            timeline.Add(
                new FireLineHeat(logoSimulator, 0.3f, 15f, 0.0f, 0.75f)
            ).SetDuration(1);

            timeline.AddGroup(
                new SimulatorBlendTo(ofogoSimulator)
            //add gradient n other
            ).SetDuration(.25f).Wait(4);

            timeline.AddGroup(
                new VectorFieldBlendTo(fixedTurbulenceVectorFieldGenerator),
                new GradientBlendTo(volumeProfile, blueGradient.gpuGradient)
            ).SetDuration(2).Wait(4);

            timeline.AddGroup(
                new SimulatorBlendTo(solarPowerSimulator),
                new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            ).SetDuration(2).Wait(4);

            timeline.AddGroup(
                  new RendererAlpha(fogoParticleRenderer, 1, 0)
            //add gradient n other
            ).SetDuration(1);
        }
    }
}