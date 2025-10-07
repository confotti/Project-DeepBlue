// This shader writes a stencil bit (HDRP-safe: use _StencilWriteMask) and depth but outputs no color.
// Create a Material from this shader and assign it to your window mask meshes (placed slightly in front of glass).


Shader "Hidden/HDRP_WindowStencil"
{
Properties
{
[HideInInspector]_StencilWriteMask("_StencilWriteMask", Float) = 0
[HideInInspector]_StencilRef("_StencilRef", Float) = 0
}
SubShader
{
// Render in the geometry queue so it participates in depth writes before transparent passes
Tags { "Queue" = "Geometry-1" "RenderPipeline" = "HDRenderPipeline" }


Pass
{
Name "STENCIL"
// don't write color
ColorMask 0
// write depth so interior geometry doesn't draw into the window pixels
ZWrite On
Cull Off


// HDRP requires you to use the user-write mask when touching stencil bits
Stencil
{
Ref [_StencilRef]
Comp Always
Pass Replace
WriteMask [_StencilWriteMask]
}
}
}
} 