using UnityEngine;

[ExecuteAlways]
public class WaterCameraProjector : MonoBehaviour
{
    public Camera mainCamera;       // usually your player/main camera
    public Camera waterCamera;      // the camera that renders exterior -> RT_Water
    public RenderTexture waterRT;   // RT_Water

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (waterCamera == null) Debug.LogWarning("Assign waterCamera.");
        if (waterRT != null && waterCamera != null) waterCamera.targetTexture = waterRT;
        Shader.SetGlobalTexture("_WaterRT", waterRT);
    }

    void LateUpdate()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null || waterCamera == null) return;

        // Keep the water camera matching the main camera transform + projection
        waterCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
        waterCamera.fieldOfView = mainCamera.fieldOfView;
        waterCamera.orthographic = mainCamera.orthographic;
        waterCamera.orthographicSize = mainCamera.orthographicSize;
        waterCamera.aspect = mainCamera.aspect;

        // Build GPU-ready view-projection matrix (rendering into a texture => true)
        Matrix4x4 gpuProj = GL.GetGPUProjectionMatrix(waterCamera.projectionMatrix, true);
        Matrix4x4 vp = gpuProj * waterCamera.worldToCameraMatrix;

        // Send to shader (global)
        Shader.SetGlobalMatrix("_WaterCameraVP", vp);
        Shader.SetGlobalTexture("_WaterRT", waterRT);
    }
}