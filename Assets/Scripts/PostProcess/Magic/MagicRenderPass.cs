using System.Collections.Generic;
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

    List<Color> colorsBuffer = new List<Color>();

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

        //material.SetFloat("_Threshold", magicSettings.alphaThreshold.value);
        material.SetFloat("_MinRemap", magicSettings.minRemap.value);
        material.SetFloat("_MaxRemap", magicSettings.maxRemap.value);

        material.SetFloat("_LumPow", magicSettings.luminocityPower.value);
        material.SetFloat("_Quantize", magicSettings.quantize.value);
        material.SetInt("_Diamondize", magicSettings.diamondize.value ? 1 : 0);
        material.SetInt("_UseDither", magicSettings.useDither.value ? 1 : 0);

        colorsBuffer.Clear();

        colorsBuffer.Add(magicSettings.col0.value);
        colorsBuffer.Add(magicSettings.col1.value);
        colorsBuffer.Add(magicSettings.col2.value);
        colorsBuffer.Add(magicSettings.col3.value);
        colorsBuffer.Add(magicSettings.col4.value);
        colorsBuffer.Add(magicSettings.col5.value);
        colorsBuffer.Add(magicSettings.col6.value);
        colorsBuffer.Add(magicSettings.col7.value);
        colorsBuffer.Add(magicSettings.col8.value);
        colorsBuffer.Add(magicSettings.col9.value);

        material.SetColorArray("_ColArray", colorsBuffer);
        material.SetInt("_ColArrayCount", magicSettings.colorCount.value);


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
