Shader "Custom/KelpMinimal"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 0, 1)
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

            StructuredBuffer<float3> _SegmentPositions;
            float4 _Color;

            struct appdata
            {
                float3 vertex : POSITION;
                uint instanceID : SV_InstanceID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 pos = _SegmentPositions[v.instanceID];

                float3 worldPos = pos + v.vertex; // No rotation yet
                o.pos = UnityObjectToClipPos(float4(worldPos, 1));

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