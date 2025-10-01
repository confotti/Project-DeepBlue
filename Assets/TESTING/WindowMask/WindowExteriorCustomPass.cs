using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

class WindowExteriorCustomPass : CustomPass
{
    [Header("Assign in Inspector")]
    public Camera exteriorCamera;           // Exterior camera rendering the world
    public LayerMask exteriorLayerMask;     // Layer mask for exterior objects
    public LayerMask windowLayerMask;       // Layer mask for windows
    public Material maskOverrideMaterial;   // Simple white unlit material for window mask
    public Material compositeMaterial;      // Shader "Hidden/WindowComposite"

    private RTHandle m_ExteriorRT;
    private RTHandle m_MaskRT;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Allocate dynamic RTs that scale with screen resolution
        m_ExteriorRT = RTHandles.Alloc(Vector2.one, name: "ExteriorRT", useDynamicScale: true);
        m_MaskRT = RTHandles.Alloc(Vector2.one, name: "WindowMaskRT",
            colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm,
            useDynamicScale: true);

        if (exteriorCamera != null)
        {
            // Make sure camera is enabled, but hidden from screen
            exteriorCamera.enabled = true;
            exteriorCamera.targetTexture = null; // we override target via Custom Pass
            exteriorCamera.cullingMask = exteriorLayerMask;
        }
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (exteriorCamera == null || compositeMaterial == null || maskOverrideMaterial == null)
            return;

        Camera mainCam = ctx.hdCamera.camera;

        // 🔄 Sync exterior camera with main camera
        exteriorCamera.transform.SetPositionAndRotation(mainCam.transform.position, mainCam.transform.rotation);
        exteriorCamera.fieldOfView = mainCam.fieldOfView;
        exteriorCamera.aspect = mainCam.aspect;
        exteriorCamera.nearClipPlane = mainCam.nearClipPlane;
        exteriorCamera.farClipPlane = mainCam.farClipPlane;

        // 1️⃣ Render exterior scene into exterior RT
        CustomPassUtils.RenderFromCamera(ctx, exteriorCamera, m_ExteriorRT,
            ClearFlag.Color, exteriorLayerMask, CustomPass.RenderQueueType.All, null);

        // 2️⃣ Render window mask from main camera into mask RT
        CoreUtils.SetRenderTarget(ctx.cmd, m_MaskRT, ClearFlag.Color, Color.black);
        CustomPassUtils.RenderFromCamera(ctx, mainCam, m_MaskRT,
            ClearFlag.Color, windowLayerMask, CustomPass.RenderQueueType.All, maskOverrideMaterial);

        // 3️⃣ Composite exterior over interior using mask
        compositeMaterial.SetTexture("_ExteriorTex", m_ExteriorRT);
        compositeMaterial.SetTexture("_MaskTex", m_MaskRT);

        // Apply fullscreen composite into the interior camera's color buffer
        HDUtils.DrawFullScreen(ctx.cmd, compositeMaterial, ctx.cameraColorBuffer);
    }

    protected override void Cleanup()
    {
        if (m_ExteriorRT != null) RTHandles.Release(m_ExteriorRT);
        if (m_MaskRT != null) RTHandles.Release(m_MaskRT);
    }
}