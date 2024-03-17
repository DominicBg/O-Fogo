using Unity.Mathematics;
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
        [SerializeField] RadialVectorField radialVectorField;
        [SerializeField] BlendVectorField fixedTurbulenceVectorFieldGenerator;
        [SerializeField] BlendVectorField radialTurbulenceVectorFieldGenerator;

        [Header("Renderers")]
        [SerializeField] FogoParticleRenderer fogoParticleRenderer;

        [Header("Calentador")]
        [SerializeField] ParedesCalientes paredesCalientes;
        [SerializeField] ElPisoEsLava pisoEsLava;

        [Header("Gradient")]
        [SerializeField] VolumeProfile volumeProfile;
        [SerializeField] GPUColorGradientScriptable fireGradient;
        [SerializeField] GPUColorGradientScriptable blueGradient;

        [Header("Video")]
        [SerializeField] bool activateLogo;
        [SerializeField] bool activateTornadoOfFuego;
        [SerializeField] bool activateNormalFire;
        [SerializeField] bool activateSolarPower;

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

            if(activateLogo)
                AddLogoSimulation(timeline);
            //timeline.AddOnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));
            //timeline.AddOnStart(() => fogoParticleRenderer.alpha = 0);

            //timeline.Add(
            //   new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            //).SetDuration(0);

            //timeline.Add(
            //    new RendererAlpha(fogoParticleRenderer, 0, 1)
            //).SetDuration(2);

            //timeline.Add(
            //    new FireLineHeat(logoSimulator, 0.3f, 15f, 0.0f, 0.75f)
            //).SetDuration(1);

            if(activateTornadoOfFuego)
                AddBlendToTornadoOfFire(timeline);

            //timeline.AddGroup(
            //    new SimulatorBlendTo(ofogoSimulator)
            ////add gradient n other
            //).SetDuration(.25f).Wait(6);

            //timeline.AddGroup(
            //    new VectorFieldBlendTo(fixedTurbulenceVectorFieldGenerator),
            //    new GradientBlendTo(volumeProfile, blueGradient.gpuGradient)
            //).SetDuration(2).Wait(6);

            if(activateNormalFire)
                AddBlendToNormalFire(timeline);

            //timeline.AddGroup(
            //    new SimulatorBlendTo(solarPowerSimulator),
            //    new VectorFieldBlendTo(radialTurbulenceVectorFieldGenerator),
            //    new GradientBlendTo(volumeProfile, fireGradient.gpuGradient),
            //    new OnUpdateAction(t => radialVectorField.force = -t * 4),
            //    new OnUpdateAction(t => radialVectorField.angle = math.lerp(70, 90, t))
            //).SetDuration(2).Wait(6);

            if(activateSolarPower)
                AddBlendToSolarPower(timeline);

            timeline.AddGroup(
                new SimulatorBlendTo(ofogoSimulator),
                new OnUpdateAction(t => radialVectorField.angle = math.lerp(90, 120, t))
            ).SetDuration(2).Wait(6);

            timeline.AddGroup(
                  new RendererAlpha(fogoParticleRenderer, 1, 0)
            //add gradient n other
            ).SetDuration(1);
        }

        void AddLogoSimulation(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));
            //timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
            //timeline.AddOnStart(() => fogoParticleRenderer.alpha = 0);
            //timeline.AddOnStart(() => radialVectorField.force = -1);

            timeline.Add(
               new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            ).SetDuration(0);

            timeline.Add(
                new RendererAlpha(fogoParticleRenderer, 0, 1)
            ).SetDuration(2);

            timeline.Add(
                new FireLineHeat(logoSimulator, 0.3f, 15f, 0.0f, 0.75f)
            ).SetDuration(1);
        }

        void AddBlendToTornadoOfFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
            timeline.AddOnStart(() => radialVectorField.force = -1);
            timeline.Add(
                 new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
              ).SetDuration(0);

            timeline.AddGroup(
                new SimulatorBlendTo(ofogoSimulator)
            //add gradient n other
            ).SetDuration(.25f).Wait(6);
        }

        void AddBlendToNormalFire(AnimationTimelineXVII timeline)
        {
            timeline.AddGroup(
                new VectorFieldBlendTo(fixedTurbulenceVectorFieldGenerator),
                new GradientBlendTo(volumeProfile, blueGradient.gpuGradient)
            )   .SetDuration(2).Wait(6);
        }

        void AddBlendToSolarPower(AnimationTimelineXVII timeline)
        {
            timeline.AddGroup(
                new SimulatorBlendTo(solarPowerSimulator),
                new VectorFieldBlendTo(radialTurbulenceVectorFieldGenerator),
                new GradientBlendTo(volumeProfile, fireGradient.gpuGradient),
                new OnUpdateAction(t => radialVectorField.force = -t * 4),
                new OnUpdateAction(t => radialVectorField.angle = math.lerp(70, 90, t))
            ).SetDuration(2).Wait(6);
        }
    }
}