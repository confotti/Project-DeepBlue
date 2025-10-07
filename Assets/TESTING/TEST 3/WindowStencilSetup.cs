using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


[ExecuteAlways]
public class WindowStencilSetup : MonoBehaviour
{
    [Tooltip("Material used to write the stencil on your window mask meshes (ColorMask 0, ZWrite On)")]
    public Material maskMaterial;


    [Tooltip("Material used by the FullScreen Custom Pass that samples the water RT")]
    public Material compositeMaterial;


    [Tooltip("Which user stencil bit to use (HDRP exposes two: UserBit0 and UserBit1)")]
    public UserStencilUsage stencilBit = UserStencilUsage.UserBit0;


    [ContextMenu("Apply Stencil Settings")]
    public void Apply()
    {
        int bitVal = (int)stencilBit; // HDRP enum -> actual numeric mask (UserBit0=64, UserBit1=128)


        if (maskMaterial != null)
        {
            maskMaterial.SetInt("_StencilWriteMask", bitVal);
            maskMaterial.SetInt("_StencilRef", bitVal);
        }


        if (compositeMaterial != null)
        {
            compositeMaterial.SetInt("_StencilReadMask", bitVal);
            compositeMaterial.SetInt("_StencilRef", bitVal);
        }
    }


    void OnValidate() => Apply();
    void Awake() => Apply();
} 