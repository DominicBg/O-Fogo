using UnityEngine.Rendering.Universal;

public class HeatEffectRendererFeature : ScriptableRendererFeature
{
    HeatEffectRenderPass renderPass;

    public override void Create()
    {
        renderPass = new HeatEffectRenderPass();
        name = "HeatEffect";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(renderPass.Setup(renderer))
        {
            renderer.EnqueuePass(renderPass);
        }
    }
}
