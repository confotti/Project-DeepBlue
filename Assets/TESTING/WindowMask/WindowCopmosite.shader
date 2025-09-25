Shader "Hidden/WindowComposite"
{
    Properties
    {
        _ExteriorTex ("ExteriorTex", 2D) = "white" {}
        _MaskTex     ("MaskTex", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
            };

            Varyings Vert(Attributes v)
            {
                Varyings o;
                // Directly pass clip-space position
                o.pos = float4(v.vertex.xy, 0.0, 1.0);
                o.uv  = v.uv;
                return o;
            }

            sampler2D _ExteriorTex;
            sampler2D _MaskTex;

            float4 Frag(Varyings i) : SV_Target
            {
                float mask = tex2D(_MaskTex, i.uv).r;

                // keep interior pixels
                if (mask < 0.5)
                    discard;

                float4 exterior = tex2D(_ExteriorTex, i.uv);
                return exterior;
            }
            ENDHLSL
        }
    }
}