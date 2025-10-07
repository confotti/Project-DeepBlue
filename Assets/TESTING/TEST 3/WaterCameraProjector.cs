using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;


[ExecuteAlways]
[HelpURL("https://your.project.docs/window-portal-hdrp")]
public class WaterCameraProjector_HDRP : MonoBehaviour
{
    [Header("Cameras & RT")]
    public Camera mainCamera; // usually your player/main camera (auto-filled)
    public Camera waterCamera; // camera that renders the exterior into waterRT
    public RenderTexture waterRT; // the RT that waterCamera renders into


    [Header("Composite Material (FullScreen Custom Pass)")]
    public Material compositeMaterial; // assigned to the FullScreen custom pass (or any fullscreen material)


    void OnEnable()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        SetupRT();
        ApplyToMaterial();
    }


    void LateUpdate()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || waterCamera == null || waterRT == null) return;


        // keep the water camera matched to the main camera transform + projection
        waterCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        waterCamera.fieldOfView = mainCamera.fieldOfView;
        waterCamera.orthographic = mainCamera.orthographic;
        waterCamera.orthographicSize = mainCamera.orthographicSize;
        waterCamera.aspect = mainCamera.aspect;
        waterCamera.nearClipPlane = mainCamera.nearClipPlane;
        waterCamera.farClipPlane = mainCamera.farClipPlane;
        waterCamera.stereoTargetEye = mainCamera.stereoTargetEye;


        if (waterCamera.targetTexture != waterRT)
            waterCamera.targetTexture = waterRT;


        Shader.SetGlobalTexture("_WaterRT", waterRT);
        ApplyToMaterial();
    }


    void SetupRT()
    {
        if (waterCamera != null && waterRT != null)
            waterCamera.targetTexture = waterRT;


        Shader.SetGlobalTexture("_WaterRT", waterRT);
    }


    void ApplyToMaterial()
    {
        if (compositeMaterial != null && waterRT != null)
        {
            compositeMaterial.SetTexture("_WaterRT", waterRT);
        }
    }


    void OnDisable()
    {
        if (waterCamera != null)
            waterCamera.targetTexture = null;
    }
} 