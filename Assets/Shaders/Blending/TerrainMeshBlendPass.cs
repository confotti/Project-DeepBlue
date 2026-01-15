using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

class TerrainMeshBlendPass : CustomPass
{
    public Material blendMaterial;

    protected override void Execute(CustomPassContext ctx)
    {
        if (blendMaterial == null) return;

        // Draw full screen quad
        CoreUtils.DrawFullScreen(ctx.cmd, blendMaterial, shaderPassId: 0);
    }
}