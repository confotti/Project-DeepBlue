using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class FogEffect : MonoBehaviour
{
    public Material mat;

    private void Start()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth; 
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mat); 
    }
}
