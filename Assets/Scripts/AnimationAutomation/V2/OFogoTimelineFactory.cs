using OFogo;
using UnityEngine;

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
        timeline.OnStart(() => OFogoController.Instance.SetSimulator(logoSimulator));
        timeline.OnStart(() => OFogoController.Instance.SetVectorFieldGenerator(radialTurbulenceVectorFieldGenerator));
        timeline.OnStart(() => fogoParticleRenderer.alpha = 0);

        timeline.Add(
            new RendererAlphaAnimationPart(fogoParticleRenderer, 0, 1)
        ).SetDuration(2);

        timeline.Add(
                new FireLineHeatAnimation(logoSimulator, 0.3f, 15f, 0.0f, 0.75f)
        ).SetDuration(2);

        timeline.AddGroup(
                new SimulatorBlendToAnimationPart(ofogoSimulator)
        //add gradient n other
        ).SetDuration(2).Wait(4);

        timeline.AddGroup(
            new VectorFieldBlendToAnimationPart(fixedTurbulenceVectorFieldGenerator)
        //add gradient n other

        ).SetDuration(2).Wait(4);
        
        timeline.AddGroup(
            new SimulatorBlendToAnimationPart(solarPowerSimulator)
        //add gradient n other
        ).SetDuration(2).Wait(4);

        timeline.AddGroup(
              new RendererAlphaAnimationPart(fogoParticleRenderer, 1, 0)
        //add gradient n other
        ).SetDuration(5);
    }
}
