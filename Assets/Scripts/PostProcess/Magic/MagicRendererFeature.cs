using UnityEngine.Rendering.Universal;

public class MagicRendererFeature : ScriptableRendererFeature
{
    MagicRenderPass blurRenderPass;

    public override void Create()
    {
        blurRenderPass = new MagicRenderPass();
        name = "Magic";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(blurRenderPass.Setup(renderer))
        {
            renderer.EnqueuePass(blurRenderPass);
        }
    }
}
