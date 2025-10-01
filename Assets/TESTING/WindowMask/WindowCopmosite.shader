Shader "Hidden/WindowComposite"
{
    Properties
    {
        _ExteriorTex ("Exterior Texture", 2D) = "white" {}
        _MaskTex     ("Window Mask", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float2 uv : TEXCOORD0; };

            // Vertex shader is a simple pass-through for UVs; HDUtils.DrawFullScreen provides positions
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.uv = v.uv;
                return o;
            }

            sampler2D _ExteriorTex;
            sampler2D _MaskTex;
            sampler2D _CameraColorTexture; // automatically bound by HDUtils.DrawFullScreen

            float4 frag(Varyings i) : SV_Target
            {
                // Sample window mask (white = window, black = interior)
                float mask = tex2D(_MaskTex, i.uv).r;

                // Sample current interior framebuffer
                float4 interior = tex2D(_CameraColorTexture, i.uv);

                // Sample exterior render texture
                float4 exterior = tex2D(_ExteriorTex, i.uv);

                // Blend exterior over interior only where mask > 0
                return lerp(interior, exterior, mask);
            }

            ENDHLSL
        }
    }
}