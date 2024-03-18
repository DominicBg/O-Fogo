using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace OFogo.Animations
{
    public class OFogoTimelineFactory : MonoBehaviour, AnimationTimelineFactory
    {
        [Header("Simulator")]
        [SerializeField] FireStrokeSimulator logoSimulator;
        [SerializeField] FireStrokeSimulator serpenteDiFuocoSimulator;
        [SerializeField] AutoMover serpenteDiFuocoMover;
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
        [SerializeField] AnimationCurve logoToFirstSimCurve;

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
            if (activateLogo)
                AddLogoSimulation(timeline);

            if (activateTornadoOfFuego)
                AddBlendToTornadoOfFire(timeline);


            if (activateNormalFire)
                AddBlendToNormalFire(timeline);

            if (activateSolarPower)
                AddBlendToSolarPower(timeline);

            AddBlendToSerpenteDiFuoco(timeline);

            timeline.AddGroup(
                new SimulatorBlendTo(ofogoSimulator),
                new OnUpdateAction(t => radialVectorField.angle = math.lerp(90, 120, t))
            ).SetDuration(4).Wait(6);

            timeline.AddGroup(
                  new RendererAlpha(fogoParticleRenderer, 1, 0)
            //add gradient n other
            ).SetDuration(2).Wait(1);
        }

        void AddLogoSimulation(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));

            timeline.Add(
               new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            ).SetDuration(0);

            timeline.Add(
                new RendererAlpha(fogoParticleRenderer, 0, 1)
            ).SetDuration(1);

            timeline.Add(
                new FireLineHeat(logoSimulator, 0.3f, 15f, 0.0f, 0.75f)
            ).SetDuration(2);
        }

        void AddBlendToTornadoOfFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
            timeline.AddOnStart(() => radialVectorField.force = -1);
            //timeline.Add(
            //     new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            //  ).SetDuration(0);

            timeline.AddGroup(
                new SimulatorBlendTo(ofogoSimulator)
            //add gradient n other
            ).SetDuration(0.25f).SetAnimationCurve(logoToFirstSimCurve).Wait(6);
        }

        void AddBlendToNormalFire(AnimationTimelineXVII timeline)
        {
            timeline.AddGroup(
                new VectorFieldBlendTo(fixedTurbulenceVectorFieldGenerator),
                new GradientBlendTo(volumeProfile, blueGradient.gpuGradient)
            ).SetDuration(2).Wait(6);
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

        void AddBlendToSerpenteDiFuoco(AnimationTimelineXVII timeline)
        {
            timeline.Add(
                  new SimulatorBlendTo(serpenteDiFuocoSimulator)
            ).SetDuration(1);

            timeline.AddGroup(
                new OnUpdateAction((t) => serpenteDiFuocoMover.radius = math.lerp(1, 9, t)),
                new OnUpdateAction((t) => serpenteDiFuocoMover.rotationSpeed = math.lerp(360 + 180, 360, t))
            ).SetDuration(5);

            timeline.AddGroup(
                new OnUpdateAction((t) => serpenteDiFuocoMover.heightSinAmplitude = math.lerp(0, 5.6f, t)),
                new OnUpdateAction((t) => serpenteDiFuocoMover.radius = math.lerp(9, 5, t))
            ).SetDuration(1).Wait(6);
        }
    }
}