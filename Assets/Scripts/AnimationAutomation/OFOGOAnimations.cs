using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace OFogo.Animations
{
    public class FireLineHeat : AnimationXVII
    {
        FireStrokeSimulator fireStrokeSimulator;
        float minHeatHeight;
        float maxHeatHeight;
        float minHeatMultiplicator;
        float maxHeatMultiplicator;


        public FireLineHeat(FireStrokeSimulator fireStrokeSimulator, float minHeatHeight, float maxHeatHeight, float minHeatMultiplicator, float maxHeatMultiplicator)
        {
            this.fireStrokeSimulator = fireStrokeSimulator;
            this.minHeatHeight = minHeatHeight;
            this.maxHeatHeight = maxHeatHeight;
            this.minHeatMultiplicator = minHeatMultiplicator;
            this.maxHeatMultiplicator = maxHeatMultiplicator;
        }

        protected override void OnEnd()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnUpdateAnimation(float timeRatio)
        {
            fireStrokeSimulator.burnHeight = math.lerp(minHeatHeight, maxHeatHeight, timeRatio);
            fireStrokeSimulator.heatMultiplicator = math.lerp(minHeatMultiplicator, maxHeatMultiplicator, timeRatio);
        }
    }
    public class RendererAlpha : AnimationXVII
    {
        AlphaRenderer alphaRenderer;
        float alphaFrom;
        float alphaTo;

        public RendererAlpha(AlphaRenderer alphaRenderer, float alphaFrom, float alphaTo)
        {
            this.alphaRenderer = alphaRenderer;
            this.alphaFrom = alphaFrom;
            this.alphaTo = alphaTo;
        }

        protected override void OnEnd()
        {
        }

        protected override void OnStart()
        {
        }

        protected override void OnUpdateAnimation(float timeRatio)
        {
            alphaRenderer.alpha = math.remap(0, 1, alphaFrom, alphaTo, timeRatio);
        }
    }

    public class SimulatorBlendTo : AnimationXVII
    {
        [SerializeField] FireParticleSimulator simulatorBlendTo;

        SimulatorBlend simulatorBlend;

        public SimulatorBlendTo(FireParticleSimulator simulatorBlendTo)
        {
            this.simulatorBlendTo = simulatorBlendTo;
        }

        public override void OnCreated()
        {
            var blendToAnim = new GameObject("SimulatorBlendToAnimation");
            simulatorBlend = blendToAnim.AddComponent<SimulatorBlend>();
        }

        protected override void OnEnd()
        {
        }

        protected override void OnStart()
        {
            simulatorBlend.fireParticleSimulatorA = OFogoController.Instance.GetCurrentSimulator();
            if(simulatorBlend.fireParticleSimulatorA is SimulatorBlend blend)
            {
                //Prevent overflow when blend blend on blend blend blend blend blend blend blend blend blend
                if (blend.ratio < 0.5f)
                    simulatorBlend.fireParticleSimulatorA = blend.fireParticleSimulatorA;
                else
                    simulatorBlend.fireParticleSimulatorA = blend.fireParticleSimulatorB;

            }

            simulatorBlend.fireParticleSimulatorB = simulatorBlendTo;

            OFogoController.Instance.SetSimulator(simulatorBlend);
        }

        protected override void OnUpdateAnimation(float timeRatio)
        {
            simulatorBlend.ratio = timeRatio;
        }
    }

    public class VectorFieldBlendTo : AnimationXVII
    {
        VectorFieldGenerator vectorFieldBlendTo;

        BlendVectorField vectorFieldBlend;

        public VectorFieldBlendTo(VectorFieldGenerator vectorFieldBlendTo)
        {
            this.vectorFieldBlendTo = vectorFieldBlendTo;
        }

        public override void OnCreated()
        {
            var blendToAnim = new GameObject("VectorFieldBlendToAnimation");
            vectorFieldBlend = blendToAnim.AddComponent<BlendVectorField>();
        }

        protected override void OnEnd()
        {
        }

        protected override void OnStart()
        {
            vectorFieldBlend.vectorFieldGeneratorA = OFogoController.Instance.GetCurrentVectorFieldGenerator();
            vectorFieldBlend.vectorFieldGeneratorB = vectorFieldBlendTo;

            OFogoController.Instance.SetVectorFieldGenerator(vectorFieldBlend);
        }

        protected override void OnUpdateAnimation(float timeRatio)
        {
            vectorFieldBlend.ratio = timeRatio;
        }
    }

    public class GradientBlendTo : AnimationXVII
    {
        MagicSettings settings;

        List<Color> fromGradient = new List<Color>();
        List<Color> toGradient = new List<Color>();
        List<Color> tempGradient = new List<Color>();

        public GradientBlendTo(VolumeProfile volumeProfile, GPUColorGradient colorGradient)
        {
            toGradient.AddRange(colorGradient.colors);
            volumeProfile.TryGet(out settings);
        }

        protected override void OnEnd()
        {
        }

        protected override void OnStart()
        {
            settings.SetGradientIntoList(ref fromGradient);
        }

        protected override void OnUpdateAnimation(float timeRatio)
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
