using UnityEngine.Rendering.Universal;

public class FrameBlurRendererFeature : ScriptableRendererFeature
{
    FrameBlurRenderPass frameBlurRenderPass;

    public override void Create()
    {
        frameBlurRenderPass = new FrameBlurRenderPass();
        name = "Frame Blur";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (frameBlurRenderPass.Setup(renderer))
        {
            renderer.EnqueuePass(frameBlurRenderPass);
        }
    }
}
