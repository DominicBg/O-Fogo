using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HeatEffectRenderPass : ScriptableRenderPass
{
    private Material material;
    private HeatEffectSettings settings;

    private RenderTargetIdentifier source;

    private RenderTargetHandle tex;
    private int texID; 


    public bool Setup(ScriptableRenderer renderer)
    {
        source = renderer.cameraColorTarget;
        settings = VolumeManager.instance.stack.GetComponent<HeatEffectSettings>();
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        if (settings != null && settings.IsActive())
        {
            material = new Material(Shader.Find("PostProcessing/HeatEffect"));
            return true;
        }

        return false;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (settings == null || !settings.IsActive())
        {
            return;
        }

        texID = Shader.PropertyToID("_HeatEffectTex");
        tex = new RenderTargetHandle();
        tex.id = texID;
        cmd.GetTemporaryRT(tex.id, cameraTextureDescriptor);

        base.Configure(cmd, cameraTextureDescriptor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings == null || !settings.IsActive())
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Heat Effect Post Process");

        material.SetFloat("_Blend", settings.blend.value);
        material.SetVector("_SinAmplitude", settings.sinAmplitude.value);
        material.SetVector("_SinFrequency", settings.sinFrequency.value);
        material.SetVector("_WaveEffect", settings.waveEffect.value);
        material.SetFloat("_VerticalScroll", settings.verticalScroll.value);

        cmd.Blit(source, texID, material, 0);
        cmd.Blit(texID, source);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(texID);
        base.FrameCleanup(cmd);
    }
}
