using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static MagicController;

namespace OFogo
{
    public class FireLineHeatAnimation : AnimationXVII
    {
        FireStrokeSimulator fireStrokeSimulator;
        float minHeatHeight;
        float maxHeatHeight;
        float minHeatMultiplicator;
        float maxHeatMultiplicator;


        public FireLineHeatAnimation(FireStrokeSimulator fireStrokeSimulator, float minHeatHeight, float maxHeatHeight, float minHeatMultiplicator, float maxHeatMultiplicator)
        {
            this.fireStrokeSimulator = fireStrokeSimulator;
            this.minHeatHeight = minHeatHeight;
            this.maxHeatHeight = maxHeatHeight;
            this.minHeatMultiplicator = minHeatMultiplicator;
            this.maxHeatMultiplicator = maxHeatMultiplicator;
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float timeRatio)
        {
            fireStrokeSimulator.burnHeight = math.lerp(minHeatHeight, maxHeatHeight, timeRatio);
            fireStrokeSimulator.heatMultiplicator = math.lerp(minHeatMultiplicator, maxHeatMultiplicator, timeRatio);
        }
    }
    public class RendererAlphaAnimationPart : AnimationXVII
    {
        AlphaRenderer alphaRenderer;
        float alphaFrom;
        float alphaTo;

        public RendererAlphaAnimationPart(AlphaRenderer alphaRenderer, float alphaFrom, float alphaTo)
        {
            this.alphaRenderer = alphaRenderer;
            this.alphaFrom = alphaFrom;
            this.alphaTo = alphaTo;
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
        }

        public override void UpdateAnimation(float timeRatio)
        {
            alphaRenderer.alpha = math.remap(timeRatio, 0, 1, alphaFrom, alphaTo);
        }
    }

    public class SimulatorBlendToAnimationPart : AnimationXVII
    {
        [SerializeField] FireParticleSimulator simulatorBlendTo;

        SimulatorBlend simulatorBlend;

        public SimulatorBlendToAnimationPart(FireParticleSimulator simulatorBlendTo)
        {
            this.simulatorBlendTo = simulatorBlendTo;
        }

        // Start is called before the first frame update
        public override void OnCreated()
        {
            var blendToAnim = new GameObject("SimulatorBlendToAnimation");
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

    public class VectorFieldBlendToAnimationPart : AnimationXVII
    {
        VectorFieldGenerator vectorFieldBlendTo;

        BlendVectorField vectorFieldBlend;

        public VectorFieldBlendToAnimationPart(VectorFieldGenerator vectorFieldBlendTo)
        {
            this.vectorFieldBlendTo = vectorFieldBlendTo;
        }

        public override void OnCreated()
        {
            var blendToAnim = new GameObject("VectorFieldBlendToAnimation");
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

    public class GradientAnimationPart : AnimationXVII
    {
        MagicSettings settings;

        List<Color> fromGradient = new List<Color>();
        List<Color> toGradient = new List<Color>();
        List<Color> tempGradient = new List<Color>();

        public GradientAnimationPart(VolumeProfile volumeProfile, GPUColorGradient colorGradient)
        {
            toGradient = colorGradient.colors;
            volumeProfile.TryGet(out settings);
        }

        public override void OnEnd()
        {
        }

        public override void OnStart()
        {
            settings.SetGradientIntoList(ref toGradient);

        }

        public override void UpdateAnimation(float timeRatio)
        {
            settings.SetGradient(LerpGradient(fromGradient, toGradient, timeRatio));     
        }

        public List<Color> LerpGradient(List<Color> gradientA, List<Color> gradientB, float t)
        {
            int count = math.min(gradientA.Count, gradientB.Count);
            tempGradient.Clear();
            for (int i = 0; i < count; i++)
            {
                Color color = Color.Lerp(gradientA[i], gradientB[i], t);
                tempGradient.Add(color);
            }
            return tempGradient;
        }
    }
}
