using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FrameBlurRenderPass : ScriptableRenderPass
{
    //private Material material;
    private FrameBlurSettings settings;

    private RenderTargetIdentifier source;

    //private RenderTargetHandle tex;
    //private int texID; 

    //private RenderTexture sourceTexture;
    private RenderTexture cacheTexture;
    
    private RenderTargetHandle tex;
    private int texID;

    private ComputeShader computeShader;
    private int mainKernel;

    public bool Setup(ScriptableRenderer renderer)
    {
        source = renderer.cameraColorTarget;

        settings = VolumeManager.instance.stack.GetComponent<FrameBlurSettings>();
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        if (settings != null && settings.IsActive())
        {
            //material = new Material(Shader.Find("PostProcessing/FrameBlur"));
            computeShader = Resources.Load<ComputeShader>("FrameBlur");
            mainKernel = computeShader.FindKernel("CSMain");
            return computeShader != null;
        }

        return false;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (settings == null || !settings.IsActive())
        {
            return;
        }


        //texID = Shader.PropertyToID("_frameblurtex");
        //tex = new RenderTargetHandle();
        //tex.id = texID;
        //cmd.GetTemporaryRT(tex.id, cameraTextureDescriptor);

        //mainBlurTexID = Shader.PropertyToID("_MainTexBlur");
        //mainBlurTex = new RenderTargetHandle();
        //mainBlurTex.id = mainBlurTexID;
        //cmd.GetTemporaryRT(mainBlurTex.id, cameraTextureDescriptor);

        texID = Shader.PropertyToID("_FrameBlurTex");
        tex = new RenderTargetHandle();
        tex.id = texID;
        cmd.GetTemporaryRT(tex.id, cameraTextureDescriptor);

        if(cacheTexture == null || cacheTexture.width != cameraTextureDescriptor.width || cacheTexture.height != cameraTextureDescriptor.height)
        {
            cacheTexture = new RenderTexture(cameraTextureDescriptor);
            cacheTexture.enableRandomWrite = true;

            cmd.Blit(source, cacheTexture);
        }
        base.Configure(cmd, cameraTextureDescriptor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (settings == null || !settings.IsActive())
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get("Frame Blur Post Process");

        computeShader.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);

        cmd.SetComputeTextureParam(computeShader, mainKernel, Shader.PropertyToID("_CacheTexture"), cacheTexture);
        cmd.SetComputeTextureParam(computeShader, mainKernel, Shader.PropertyToID("_SourceTexture"), source);
        
        cmd.SetComputeFloatParam(computeShader, Shader.PropertyToID("_Blend"), settings.blend.value);
        
        cmd.DispatchCompute(computeShader, mainKernel,
              (int)math.ceil(cacheTexture.width / 8), //xGroupSize
              (int)math.ceil(cacheTexture.height / 8), //yGroupSize
              1);

        cmd.Blit(cacheTexture, source);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
    }
}
