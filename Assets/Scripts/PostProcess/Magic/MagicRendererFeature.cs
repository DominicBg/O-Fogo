using UnityEngine.Rendering.Universal;

public class MagicRendererFeature : ScriptableRendererFeature
{
    MagicRenderPass magicRenderPass;

    public override void Create()
    {
        magicRenderPass = new MagicRenderPass();
        name = "Magic";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(magicRenderPass.Setup(renderer))
        {
            renderer.EnqueuePass(magicRenderPass);
        }
    }
}
