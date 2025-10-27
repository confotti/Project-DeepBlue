Shader "Hidden/TerrainBlend/Depth"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            Name "DepthPass"
            Tags { "LightMode" = "Always" }

            HLSLINCLUDE
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos   : TEXCOORD0;
            };

            float TB_SCALE;
            float TB_OFFSET_X;
            float TB_OFFSET_Z;
            float TB_OFFSET_Y;
            float TB_FARCLIP;

            v2f vert(appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, float4(v.positionOS, 1.0));
                o.worldPos = worldPos.xyz;
                o.positionCS = TransformWorldToHClip(o.worldPos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // Convert world-space Y into normalized depth (0–1)
                // TB_OFFSET_Y = bottom of capture volume (cameraY - farClip)
                // TB_FARCLIP  = capture range in Y (far clip plane distance)
                float normalizedDepth = saturate((i.worldPos.y - TB_OFFSET_Y) / TB_FARCLIP);

                // Output as grayscale — use the same channel for R/G/B
                return float4(normalizedDepth, normalizedDepth, normalizedDepth, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}