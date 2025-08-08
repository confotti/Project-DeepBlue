Shader "Debug/InstanceTest"
{
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma multi_compile_instancing

			#include "UnityCG.cginc" 

            StructuredBuffer<float3> _TestPositions;

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
                float3 worldPos = _TestPositions[v.instanceID] + v.vertex;
                o.pos = UnityObjectToClipPos(float4(worldPos, 1));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1,0,0,1);
            }
            ENDHLSL
        }
    }
} 