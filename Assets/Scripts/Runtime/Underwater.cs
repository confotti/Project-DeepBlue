using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Underwater : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
        public Color color;
        public float distance = 10f;

        [Range(0, 1)]
        public float alpha;

        public float refraction = 0.1f;
        public Texture normalmap;
        public Vector4 UV = new Vector4(1, 1, 0.2f, 0.1f);
    }

    public Settings settings = new Settings();

    class Pass : ScriptableRenderPass
    {
        public Settings settings;
        private RTHandle tempTexture;
        private RTHandle source;

        private string profilerTag;

        public Pass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public void Setup(RTHandle source) 
        {
            this.source = source;
        } 

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_UnderwaterTempTexture");

            // Configure the tempTexture as the render target for this pass
            ConfigureTarget(tempTexture);
            ConfigureClear(ClearFlag.None, Color.clear); 
        } 

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (settings.material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            try
            {
                settings.material.SetFloat("_dis", settings.distance);
                settings.material.SetFloat("_alpha", settings.alpha);
                settings.material.SetColor("_color", settings.color);
                settings.material.SetTexture("_NormalMap", settings.normalmap);
                settings.material.SetFloat("_refraction", settings.refraction);
                settings.material.SetVector("_normalUV", settings.UV);

                // Blit using Blitter (URP 14+ standard)
                Blitter.BlitCameraTexture(cmd, source, tempTexture, settings.material, 0);
                Blitter.BlitCameraTexture(cmd, tempTexture, source);
                context.ExecuteCommandBuffer(cmd);
            }
            catch (Exception ex)
            {
                Debug.LogError("Underwater Pass Error: " + ex.Message);
            }

            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // No explicit cleanup needed as RTHandles are managed
        }
    }

    private Pass pass;

    public override void Create()
    {
        name = "Underwater Effects";
        pass = new Pass(name)
        {
            settings = settings,
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        renderingData.cameraData.requiresDepthTexture = true; 

        var cameraColorTarget = renderer.cameraColorTargetHandle;
        pass.Setup(cameraColorTarget);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
            return;

        renderer.EnqueuePass(pass); 
    }
}