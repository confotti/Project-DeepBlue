Shader "Hidden/WindowMask"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            ZWrite Off
            ZTest Always
            Cull Off
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Unity automatically injects these matrix uniforms
            float4x4 unity_ObjectToWorld;
            float4x4 unity_MatrixVP;

            struct Attributes
            {
                float3 positionOS : POSITION;   // object-space position
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION; // clip-space position
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                float4 positionWS = mul(unity_ObjectToWorld, float4(v.positionOS, 1.0));
                o.positionCS = mul(unity_MatrixVP, positionWS);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                // Output pure white into the red channel (mask)
                return float4(1,1,1,1);
            }

            ENDHLSL
        }
    }
}