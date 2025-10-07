using UnityEngine;

[ExecuteInEditMode]
public class ReplacmentShaderEffect : MonoBehaviour
{
    public Shader ReplacementShader;

    private void OnEnable()
    {
        if (ReplacementShader != null)
            GetComponent<Camera>().SetReplacementShader(ReplacementShader, "RenderType"); 
    }

    private void OnDisable()
    {
        GetComponent<Camera>().ResetReplacementShader(); 
    }
}
