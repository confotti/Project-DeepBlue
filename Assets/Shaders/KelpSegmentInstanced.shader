Shader "Custom/KelpRender"
{
    Properties
    {
        _Color ("Color", Color) = (0,1,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            StructuredBuffer<float3> _Positions; // You can define structured buffer to match stalk nodes
            StructuredBuffer<float3> _Directions;

            float4 _Color;

            struct appdata
            {
                float3 vertex : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // Fetch position and direction from buffer by instance ID
                float3 pos = _Positions[v.instanceID];
                float3 dir = _Directions[v.instanceID];

                // Simple transform: offset vertex along dir
                float3 worldPos = pos + v.vertex;

                o.pos = UnityObjectToClipPos(float4(worldPos,1));
                o.uv = v.vertex.xy;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
} 