Shader "Hidden/ShowDepth_GenericFixed"
{
    HLSLINCLUDE

    sampler2D _CameraDepthTexture;
    float4 _ZBufferParams;
    float4 _ScreenParams;

    struct Attributes
    {
        uint vertexID : SV_VertexID;
    };

    struct Varyings
    {
        float4 positionCS : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    Varyings Vert(Attributes input)
    {
        Varyings o;
        // Fullscreen triangle
        float2 verts[3] = {
            float2(-1.0, -1.0),
            float2(-1.0, 3.0),
            float2(3.0, -1.0)
        };
        o.positionCS = float4(verts[input.vertexID], 0.0, 1.0);
        o.uv = 0.5 * (o.positionCS.xy + 1.0);
        return o;
    }

    // Convert hardware depth to linear 0–1 range
    float Linear01Depth(float rawDepth, float4 zParams)
    {
        // Matches Unity’s Linear01Depth formula
        return 1.0 / (zParams.x * rawDepth + zParams.y);
    }

    float4 Frag(Varyings i) : SV_Target
    {
        // Manual depth texture sampling
        float rawDepth = tex2D(_CameraDepthTexture, i.uv).r;

        // Convert to linear range (near = 0, far = 1)
        float depth01 = saturate(Linear01Depth(rawDepth, _ZBufferParams));

        // Invert so near = white, far = black
        float invert = 1.0 - depth01;

        return float4(invert.xxx, 1.0);
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            Name "ShowDepth_GenericFixed"
            ZWrite Off
            ZTest Always
            Blend Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}