using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DarknessMaskFeature : ScriptableRendererFeature
{
    class DarknessMaskPass : ScriptableRenderPass
    {
        public Material darknessMat;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (darknessMat == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("DarknessMaskPass");
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, darknessMat);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material darknessMaterial;
    }

    public Settings settings = new Settings();
    DarknessMaskPass maskPass;

    public override void Create()
    {
        maskPass = new DarknessMaskPass {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        maskPass.darknessMat = settings.darknessMaterial;
        renderer.EnqueuePass(maskPass);
    }
}
