Shader "Custom/WindowProjectedView"
{
    Properties
    {
        _Tint("Tint", Color) = (1,1,1,1)
        _WaterRT("Water RenderTex", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _WaterRT;
            float4 _Tint;
            float4x4 _WaterCameraVP; // set from C# script

            struct app { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float4 proj : TEXCOORD0; float2 uv : TEXCOORD1; };

            v2f vert(app v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // for correct depth w.r.t interior
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.proj = mul(_WaterCameraVP, worldPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                // projection -> UV
                float2 projUV = i.proj.xy / i.proj.w * 0.5 + 0.5;

                // flip Y if needed on some platforms (Unity macro could be used)
                #ifdef UNITY_UV_STARTS_AT_TOP
                    projUV.y = 1 - projUV.y;
                #endif

                float4 col = tex2D(_WaterRT, projUV) * _Tint;
                return col;
            } 
            ENDHLSL
        }
    }
}