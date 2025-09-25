using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

class WindowExteriorCustomPass : CustomPass
{
    // Assign these in the inspector:
    public Camera exteriorCamera;           // disabled camera you created
    public LayerMask exteriorLayerMask;     // layer mask for Exterior
    public LayerMask windowMaskLayer;       // layer mask for WindowMask
    public Material maskOverrideMaterial;   // simple white unlit material
    public Material compositeMaterial;      // shader "Hidden/WindowComposite"

    RTHandle m_ExteriorRT;
    RTHandle m_MaskRT;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // Allocate two RTHandles that scale with screen size
        m_ExteriorRT = RTHandles.Alloc(Vector2.one, name: "ExteriorRT", useDynamicScale: true);
        m_MaskRT = RTHandles.Alloc(Vector2.one, name: "WindowMaskRT", colorFormat: UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UNorm, useDynamicScale: true);
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (exteriorCamera == null || compositeMaterial == null || maskOverrideMaterial == null)
            return;

        // 1) Sync the exterior camera transform/projection with the main camera for correct parallax
        Camera mainCam = ctx.hdCamera.camera;
        exteriorCamera.transform.position = mainCam.transform.position;
        exteriorCamera.transform.rotation = mainCam.transform.rotation;
        exteriorCamera.fieldOfView = mainCam.fieldOfView;
        exteriorCamera.aspect = mainCam.aspect;

        // 2) Render the exterior (the world) from the exteriorCamera into m_ExteriorRT.
        //    This uses the exteriorCamera's Volume Layer Mask and postprocess settings, so it will get the global underwater look.
        CustomPassUtils.RenderFromCamera(ctx, exteriorCamera, m_ExteriorRT, ClearFlag.Color, exteriorLayerMask, CustomPass.RenderQueueType.All, null);

        // 3) Render the window geometry into the mask RT using the override material (white)
        CustomPassUtils.RenderFromCamera(ctx, exteriorCamera, m_MaskRT, ClearFlag.Color, windowMaskLayer, CustomPass.RenderQueueType.All, maskOverrideMaterial);

        // 4) Composite: set material textures and draw fullscreen into the camera color buffer
        compositeMaterial.SetTexture("_ExteriorTex", m_ExteriorRT);
        compositeMaterial.SetTexture("_MaskTex", m_MaskRT);

        // Draw to the active camera color buffer (main camera). This replaces pixel where mask > 0.
        // HDUtils.DrawFullScreen is convenient for this. (It respects viewport automatically.)
        HDUtils.DrawFullScreen(ctx.cmd, compositeMaterial, ctx.cameraColorBuffer);
    }

    protected override void Cleanup()
    {
        if (m_ExteriorRT != null) RTHandles.Release(m_ExteriorRT);
        if (m_MaskRT != null) RTHandles.Release(m_MaskRT);
    }
}