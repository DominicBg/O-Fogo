using System.Collections.Generic;
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

        [Header("Post Process")]
        [SerializeField] VolumeProfile volumeProfile;
        [SerializeField] GPUColorGradientScriptable fireGradient;
        [SerializeField] GPUColorGradientScriptable blueGradient;
        [SerializeField] float blackLineLow = 0.3f;
        [SerializeField] float blackLineMid = 0.75f;
        [SerializeField] float blackLineHigh = 0.85f;

        [Header("Grid")]
        [SerializeField] FireStrokeSimulator gridOfFire;
        [SerializeField] LineFireStroke[] verticalLines;
        [SerializeField] LineFireStroke[] horizontalLines;

        [Header("Dancer")]
        [SerializeField] OFogoController secondaryFogoController;
        [SerializeField] Material dancerMaterial;
        [SerializeField] Animator dancerAnimator;
        [SerializeField] FogoParticleRenderer dancerRenderer;

        [Header("Video")]
        [SerializeField] AnimationCurve logoToFirstSimCurve;
        [SerializeField] float desiredBPM = 140;

        AnimationTimelineController animationTimelineController;
        float defaultSimulationSpeed;
        MagicSettings magicSettings;

        void Awake()
        {
            Time.timeScale = desiredBPM / 120;

            defaultSimulationSpeed = OFogoController.Instance.simulationSpeed;

            animationTimelineController = new AnimationTimelineController(this);
            animationTimelineController.Start(cycleAnimations: true);

            dancerMaterial.color = Color.clear;

            volumeProfile.TryGet(out magicSettings);
        }

        void Update()
        {
            animationTimelineController.Update(Time.deltaTime);
        }

        public void CreateAnimationTimeLine(AnimationTimelineXVII timeline)
        {
            StartCleanup(timeline);

            AddChurrascoDoFogo(timeline);

            AddLogoSimulation(timeline);
            AddBlendToTornadoOfFire(timeline);
            AddBlendToNormalFire(timeline);

            AddBlendToSerpenteDiFuoco(timeline);
            AddBlendToDanceFire(timeline);
            AddVectorFieldFire(timeline);
        }

        void StartCleanup(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => secondaryFogoController.enabled = false);
            timeline.AddOnStart(() => dancerAnimator.enabled = false);
            timeline.AddOnStart(() => dancerRenderer.alpha = 0);
        }

        void AddChurrascoDoFogo(AnimationTimelineXVII timeline)
        {
            const float delay = 0.1f;
            List<AnimationXVII> updateActions = new List<AnimationXVII>();
            for (int i = 0; i < verticalLines.Length; i++)
            {
                int copyI = i;
                OnUpdateAction verticalLine = new OnUpdateAction(t => verticalLines[copyI].fireStroke.posB.y = math.lerp(-1, 1, t));
                verticalLine.SetDelay(copyI * delay).SetDuration(3);

                updateActions.Add(verticalLine);
            }
            OnUpdateAction[] horizontalLineAnims = new OnUpdateAction[horizontalLines.Length];
            for (int i = 0; i < horizontalLines.Length; i++)
            {
                int copyI = i;
                OnUpdateAction horizontalLineAction = new OnUpdateAction(t => horizontalLines[copyI].fireStroke.posB.x = math.lerp(-1, 1, t));
                horizontalLineAction.SetDelay((copyI + +verticalLines.Length) * delay).SetDuration(3);

                updateActions.Add(horizontalLineAction);
            }

            var burnAction = new OnUpdateAction(t => gridOfFire.heatMultiplicator = math.lerp(0, 0.75f, t));
            burnAction.SetDuration(4);
            updateActions.Add(burnAction);

            timeline.Add(
                updateActions
            ).SetDuration(4).Wait(4);
        }

        void AddLogoSimulation(AnimationTimelineXVII timeline)
        {
            float startBurnHeight = 3f;
            float endBurnHeight = 15f;
            timeline.AddOnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));
            timeline.AddOnStart(() => logoSimulator.burnHeight = startBurnHeight);
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(paredesCalientes));
            timeline.AddOnStart(() => OFogoController.Instance.simulationSpeed = defaultSimulationSpeed);
            timeline.AddOnStart(() => magicSettings.minNoise.value = blackLineHigh);

            timeline.Add(
               new GradientBlendTo(volumeProfile, fireGradient.gpuGradient)
            ).SetDuration(0);

            timeline.Add(
                new RendererAlpha(fogoParticleRenderer, .2f, 1),
                new FireLineHeat(logoSimulator, startBurnHeight, endBurnHeight, logoSimulator.heatMultiplicator, 0.75f)
            ).SetDuration(4).SetEaseCurve(EaseXVII.Ease.InQuint);
        }

        void AddBlendToTornadoOfFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
            timeline.AddOnStart(() => radialVectorField.force = -1);
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(paredesCalientes));

            timeline.Add(
                new SimulatorBlendTo(ofogoSimulator)
            //new FireLineHeat(logoSimulator, 15, 5f, 0.75f, 0.75f),
            //new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(4, defaultSimulationSpeed, t))
            //add gradient n other
            ).SetDuration(1f).SetAnimationCurve(logoToFirstSimCurve).Wait(7);
        }

        void AddBlendToNormalFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(pisoEsLava));

            timeline.Add(
                new SimulatorBlendTo(feuNormal),
                new VectorFieldBlendTo(turbulenceVectorFieldGenerator),
                new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(defaultSimulationSpeed, 3, t)),
                new OnUpdateAction(t => magicSettings.minNoise.value = math.lerp(blackLineHigh, blackLineMid, t))
            //new GradientBlendTo(volumeProfile, blueGradient.gpuGradient)
            ).SetDuration(2).Wait(4);
        }

        void AddBlendToSerpenteDiFuoco(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.simulationSpeed = defaultSimulationSpeed);
            timeline.AddOnStart(() => serpenteDiFuocoMover.radius = 3);
            timeline.AddOnStart(() => serpenteDiFuocoMover.heightSinAmplitude = 0);
            timeline.AddOnStart(() => serpenteDiFuocoMover.heightSinOffset = 0);
            timeline.AddOnStart(() => serpenteDiFuocoMover.ClearPoints());

            timeline.Add(
                new SimulatorBlendTo(serpenteDiFuocoSimulator),
                new OnUpdateAction(t => magicSettings.minNoise.value = math.lerp(blackLineMid, blackLineLow, t))
            ).SetEaseCurve(EaseXVII.Ease.InQuint).SetDuration(2);

            timeline.Add(
                new OnUpdateAction((t) => serpenteDiFuocoMover.radius = math.lerp(3, 9, t))
            ).SetDuration(4);

            timeline.Add(
                new OnUpdateAction((t) => serpenteDiFuocoMover.heightSinAmplitude = math.lerp(0, 5.6f, t)),
                new OnUpdateAction((t) => serpenteDiFuocoMover.radius = math.lerp(9, 5, t)),
                new OnUpdateAction(t => magicSettings.minNoise.value = math.lerp(blackLineLow, blackLineMid, t))
            ).SetDuration(2);

            timeline.Add(
                new OnUpdateAction(t => serpenteDiFuocoMover.heightSinOffset = math.lerp(0, -math.PI * 6, t))
            ).SetDuration(2);
        }

        void AddBlendToDanceFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => OFogoController.Instance.SetCalentador(pisoEsLava));
            timeline.AddOnStart(() => secondaryFogoController.enabled = true);
            timeline.AddOnStart(() => dancerAnimator.enabled = true);
            timeline.AddOnStart(() => dancerRenderer.alpha = 0);

            timeline.Add(
                new SimulatorBlendTo(feuNormal),
                new VectorFieldBlendTo(turbulenceVectorFieldGenerator),
                new OnUpdateAction(t => OFogoController.Instance.simulationSpeed = math.lerp(defaultSimulationSpeed, 3, t)),
                new RendererAlpha(dancerRenderer, 0, 1)
            ).SetDuration(3);

            timeline.Add(
                    new OnUpdateAction(t => dancerMaterial.color = Color.Lerp(Color.clear, Color.white * 0.25f, t))
            ).Wait(4.5f);

            timeline.AddOnStart(() => dancerMaterial.color = Color.clear);
            timeline.AddOnStart(() => secondaryFogoController.SetSimulator(feuNormal)).Wait(5.5f);

            timeline.Add(
                 new RendererAlpha(fogoParticleRenderer, 1, 0),
                 new RendererAlpha(dancerRenderer, 1, 0)
            ).SetDuration(8);

            timeline.AddOnStart(() => dancerAnimator.enabled = false);
            timeline.AddOnStart(() => secondaryFogoController.enabled = false);
        }

        void AddVectorFieldFire(AnimationTimelineXVII timeline)
        {
            timeline.AddOnStart(() => triangleParticleRenderer.particleScaleMultiplier = 0.5f);

            //timeline.AddGroup(
            //  new RendererAlpha(fogoParticleRenderer, 1, 0),
            //  new RendererAlpha(dancerRenderer, 1, 0)
            //).SetDuration(2);

            //timeline.AddOnStart(() => dancerAnimator.enabled = false);
            //timeline.AddOnStart(() => secondaryFogoController.enabled = false);

            //timeline.AddGroup(
            //    new VectorFieldBlendTo(radialTurbulenceVectorFieldGenerator)
            //).SetDuration(1);
            timeline.AddOnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));

            timeline.Add(
                new RendererAlpha(triangleParticleRenderer, 0, 1),
                new OnUpdateAction((t) => triangleParticleRenderer.particleScaleMultiplier = math.lerp(0.25f, 2.5f, t))
            ).SetDuration(4);

            timeline.Add(
                new RendererAlpha(triangleParticleRenderer, 1, 0)
            ).SetDuration(3).Wait(1);
        }

    }
}