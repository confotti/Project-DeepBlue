Shader "Custom/KelpSegmentSmoothed"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _StartPos)
                UNITY_DEFINE_INSTANCED_PROP(float4, _EndPos)
                UNITY_DEFINE_INSTANCED_PROP(float4, _PrevPos)
                UNITY_DEFINE_INSTANCED_PROP(float4, _NextPos)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;

                float3 start = UNITY_ACCESS_INSTANCED_PROP(Props, _StartPos).xyz;
                float3 end   = UNITY_ACCESS_INSTANCED_PROP(Props, _EndPos).xyz;
                float3 prev  = UNITY_ACCESS_INSTANCED_PROP(Props, _PrevPos).xyz;
                float3 next  = UNITY_ACCESS_INSTANCED_PROP(Props, _NextPos).xyz;

                float3 dir = end - start;
                float len = length(dir);

                // Default directions
                float3 dirStart = normalize(start - prev);
                float3 dirEnd = normalize(next - end);
                float3 dirCurrent = normalize(dir);

                // Interpolate direction based on Z position
                float z = v.vertex.z;

                float3 smoothDir = dirCurrent;

                if (z < 0) // Start cap
                {
                    smoothDir = normalize(dirStart + dirCurrent);
                }
                else if (z > 0) // End cap
                {
                    smoothDir = normalize(dirCurrent + dirEnd);
                }

                // Rotation basis from smoothDir
                float3 up = float3(0, 1, 0);
                float3 right = normalize(cross(up, smoothDir));
                float3 newUp = cross(smoothDir, right);

                float3 local = v.vertex.xyz;
                local.z *= len;

                // Rotate using new basis
                float3 rotated = right * local.x + newUp * local.y + smoothDir * local.z;

                float3 worldPos = start + rotated;

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
