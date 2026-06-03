using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class DarknessMaskFeature : ScriptableRendererFeature
{
    class DarknessMaskPass : ScriptableRenderPass
    {
        public Material darknessMat;

        // Modern implementation using RenderGraph (Unity 2022+)
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (darknessMat == null) return;

            var resourceData = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (resourceData == null || cameraData == null) return;

            // Create pass data
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Darkness Mask Pass", out var passData))
            {
                passData.material = darknessMat;
                passData.cameraColorTarget = resourceData.activeColorTexture;

                // Setup render target
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.AllowPassCulling(false);

                // Set render function
                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.cameraColorTarget, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }

        private class PassData
        {
            public Material material;
            public TextureHandle cameraColorTarget;
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
        if (settings.darknessMaterial == null) return;
        
        maskPass.darknessMat = settings.darknessMaterial;
        renderer.EnqueuePass(maskPass);
    }
}
