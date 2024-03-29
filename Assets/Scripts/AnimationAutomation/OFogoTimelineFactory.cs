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
        [SerializeField] SerpenteDiFuoco serpenteDiFuocoMover;
        [SerializeField] OFogoSimulator ofogoSimulator;
        [SerializeField] OFogoSimulator feuNormal;

        [Header("VectorField")]
        [SerializeField] RadialVectorField radialVectorField;
        [SerializeField] BlendVectorField fixedTurbulenceVectorFieldGenerator;
        [SerializeField] TurbulenceVectorField turbulenceVectorFieldGenerator;
        [SerializeField] BlendVectorField radialTurbulenceVectorFieldGenerator;

        [Header("Renderers")]
        [SerializeField] FogoParticleRenderer fogoParticleRenderer;
        [SerializeField] TriangleVectorFieldRenderer triangleParticleRenderer;

        [Header("Calentador")]
        [SerializeField] ParedesCalientes paredesCalientes;
        [SerializeField] ElPisoEsLava pisoEsLava;

        [Header("Gradient")]
        [SerializeField] VolumeProfile volumeProfile;
        [SerializeField] GPUColorGradientScriptable fireGradient;
        [SerializeField] GPUColorGradientScriptable blueGradient;

        [Header("Dancer")]
        [SerializeField] OFogoController secondaryFogoController;
        [SerializeField] Material dancerMaterial;
        [SerializeField] Animator dancerAnimator;
        [SerializeField] FogoParticleRenderer dancerRenderer;

        [Header("Video")]
        [SerializeField] AnimationCurve logoToFirstSimCurve;

        AnimationTimelineController animationTimelineController;
        float defaultSimulationSpeed;

        void Awake()
        {
            defaultSimulationSpeed = OFogoController.Instance.simulationSpeed;

            animationTimelineController = new AnimationTimelineController(this);
            animationTimelineController.Start(cycleAnimations: true);

            dancerMaterial.color = Color.clear;
        }

        void Update()
        {
            animationTimelineController.Update(Time.deltaTime);
        }

        public void CreateAnimationTimeLine(AnimationTimelineXVII timeline)
        {
            AddLogoSimulation(timeline);
            AddBlendToTornadoOfFire(timeline);
            AddBlendToNormalFire(timeline);

            AddBlendToSerpenteDiFuoco(timeline);
            AddBlendToDanceFire(timeline);
            AddVectorFieldFire(timeline);
        }

        void AddLogoSimulation(AnimationTimelineXVII timeline)
        {
            float startBurnHeight = 0.3f;
            float endBurnHeight = 15f;
            timeline.AddOnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));
            timeline.AddOnStart(() => logoSimulator.burnHeight = startBurnHeight);
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(paredesCalientes));
            timeline.AddOnStart(() => OFogoController.Instance.simulationSpeed = defaultSimulationSpeed);

            timeline.Add(
               new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            ).SetDuration(0);

            timeline.Add(
                new RendererAlpha(fogoParticleRenderer, 0, 1)
            ).SetDuration(1);

            timeline.Add(
                new FireLineHeat(logoSimulator, startBurnHeight, endBurnHeight, 0.0f, 0.75f)
            ).SetDuration(2);
        }

        void AddBlendToTornadoOfFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
            timeline.AddOnStart(() => radialVectorField.force = -1);
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(paredesCalientes));

            timeline.AddGroup(
                new SimulatorBlendTo(ofogoSimulator),
                new FireLineHeat(logoSimulator, 15, 5f, 0.75f, 0.75f),
                new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(4, defaultSimulationSpeed, t))
            //add gradient n other
            ).SetDuration(.5f).SetAnimationCurve(logoToFirstSimCurve).Wait(6);
        }

        void AddBlendToNormalFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(pisoEsLava));

            timeline.AddGroup(
                new SimulatorBlendTo(feuNormal),
                new VectorFieldBlendTo(turbulenceVectorFieldGenerator),
                new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(defaultSimulationSpeed, 3, t))
            //new GradientBlendTo(volumeProfile, blueGradient.gpuGradient)
            ).SetDuration(2).Wait(6);
        }

        //void AddBlendToSolarPower(AnimationTimelineXVII timeline)
        //{
        //    timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(paredesCalientes));

        //    timeline.AddGroup(
        //        new SimulatorBlendTo(solarPowerSimulator),
        //        new VectorFieldBlendTo(radialTurbulenceVectorFieldGenerator),
        //        new GradientBlendTo(volumeProfile, fireGradient.gpuGradient),
        //        new OnUpdateAction(t => radialVectorField.force = -t * 4),
        //        new OnUpdateAction(t => radialVectorField.angle = math.lerp(70, 90, t)),
        //        new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(3, defaultSimulationSpeed, t))
        //    ).SetDuration(2).Wait(6);
        //}

        void AddBlendToSerpenteDiFuoco(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.simulationSpeed = defaultSimulationSpeed);
            timeline.AddOnStart(() => serpenteDiFuocoMover.radius = 3);
            timeline.AddOnStart(() => serpenteDiFuocoMover.heightSinAmplitude = 0);
            timeline.AddOnStart(() => serpenteDiFuocoMover.heightSinOffset = 0);
            //timeline.AddOnStart(() => serpenteDiFuocoMover.rotationSpeed = 180); ;
            timeline.AddOnStart(() => serpenteDiFuocoMover.ClearPoints());

            timeline.AddGroup(
                new SimulatorBlendTo(serpenteDiFuocoSimulator)
            ).SetEaseCurve(EaseXVII.Ease.InQuint).SetDuration(3);

            timeline.AddGroup(
                new OnUpdateAction((t) => serpenteDiFuocoMover.radius = math.lerp(3, 9, t))
            //new OnUpdateAction((t) => serpenteDiFuocoMover.rotationSpeed = math.lerp(180, 360, t))
            ).SetDuration(3);

            timeline.AddGroup(
                new OnUpdateAction((t) => serpenteDiFuocoMover.heightSinAmplitude = math.lerp(0, 5.6f, t)),
                new OnUpdateAction((t) => serpenteDiFuocoMover.radius = math.lerp(9, 5, t))
            ).SetDuration(2);

            timeline.Add(
                new OnUpdateAction(t => serpenteDiFuocoMover.heightSinOffset = math.lerp(0, -math.PI * 6, t))
            ).SetDuration(4);
        }

        void AddBlendToDanceFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(pisoEsLava));
            timeline.AddOnStart(() => secondaryFogoController.enabled = true);
            timeline.AddOnStart(() => dancerAnimator.enabled = true);
            timeline.AddOnStart(() => dancerRenderer.alpha = 0);

            timeline.AddGroup(
                new SimulatorBlendTo(feuNormal),
                new VectorFieldBlendTo(turbulenceVectorFieldGenerator),
                new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(defaultSimulationSpeed, 3, t)),
                new RendererAlpha(dancerRenderer, 0, 1)
            ).SetDuration(2);

            timeline.AddGroup(
                    new OnUpdateAction(t => dancerMaterial.color = Color.Lerp(Color.clear, Color.white * 0.25f, t))
            ).Wait(5);

            timeline.AddOnStart(() => secondaryFogoController.SetSimulator(feuNormal));
            timeline.AddOnStart(() => dancerMaterial.color = Color.clear);
        }

        void AddVectorFieldFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => triangleParticleRenderer.particleScaleMultiplier = 0.5f);
           // timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));

            timeline.AddGroup(
                new RendererAlpha(fogoParticleRenderer, 1, 0),
                new RendererAlpha(dancerRenderer, 1, 0),
                new RendererAlpha(triangleParticleRenderer, 0, 1)
            ).SetDuration(1);


            timeline.AddOnStart(() => dancerAnimator.enabled = false);
            timeline.AddOnStart(() => secondaryFogoController.enabled = false);

            timeline.AddGroup(
                new VectorFieldBlendTo(radialTurbulenceVectorFieldGenerator)
            ).SetDuration(1);

            timeline.AddGroup(
               new OnUpdateAction((t) => triangleParticleRenderer.particleScaleMultiplier = math.lerp(0.5f, 4.5f, t))
           ).SetDuration(4);

            timeline.AddGroup(
                new RendererAlpha(triangleParticleRenderer, 1, 0)
            ).SetDuration(2).Wait(1);
        }

    }
}