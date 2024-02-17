using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MagicRenderPass : ScriptableRenderPass
{
    private Material material;
    private MagicSettings magicSettings;

    private RenderTargetIdentifier source;

    private RenderTargetHandle magicTex;
    private int magicTexID;

    public bool Setup(ScriptableRenderer renderer)
    {
        source = renderer.cameraColorTarget;
        magicSettings = VolumeManager.instance.stack.GetComponent<MagicSettings>();
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        if (magicSettings != null && magicSettings.IsActive())
        {
            material = new Material(Shader.Find("PostProcessing/Magic"));
            return true;
        }

        return false;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (magicSettings == null || !magicSettings.IsActive())
        {
            return;
        }


        magicTexID = Shader.PropertyToID("_MagicTex");
        magicTex = new RenderTargetHandle();
        magicTex.id = magicTexID;
        cmd.GetTemporaryRT(magicTex.id, cameraTextureDescriptor);

        base.Configure(cmd, cameraTextureDescriptor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (magicSettings == null || !magicSettings.IsActive())
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Magic Post Process");

        material.SetFloat("_Threshold", magicSettings.alphaThreshold.value);
        material.SetFloat("_LumPow", magicSettings.luminocityPower.value);

        cmd.Blit(source, magicTexID, material, 0);
        cmd.Blit(magicTexID, source);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(magicTexID);
        base.FrameCleanup(cmd);
    }
}
