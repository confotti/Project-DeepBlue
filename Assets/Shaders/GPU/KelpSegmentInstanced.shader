Shader "Custom/KelpSegmentInstanced" 
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SCREEN
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct StalkNode
            {
                float3 currentPos;
                float padding0;

                float3 previousPos;
                float padding1;

                float3 direction;
                float padding2;

                float4 color;
                float bendAmount;
                float3 padding3;

                int isTip;
                float3 padding4;
            };

            StructuredBuffer<StalkNode> _StalkNodesBuffer;

            float3 _WorldOffset;
            float4 _BaseColor;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : COLOR;
                float4 shadowCoord : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                StalkNode node = _StalkNodesBuffer[input.instanceID];

                float3 dir = normalize(node.direction);
                dir = all(abs(dir) < 0.001) ? float3(0,1,0) : dir;

                float3 up = float3(0,1,0);
                float3 axis = cross(up, dir);
                float angle = acos(saturate(dot(up, dir)));

                float s = sin(angle);
                float c = cos(angle);
                float t = 1 - c;

                float3x3 rotationMatrix;

                if (length(axis) < 0.001)
                {
                    rotationMatrix = float3x3(1,0,0, 0,1,0, 0,0,1);
                }
                else
                {
                    axis = normalize(axis);
                    rotationMatrix = float3x3(
                        t*axis.x*axis.x + c,         t*axis.x*axis.y - s*axis.z,  t*axis.x*axis.z + s*axis.y,
                        t*axis.x*axis.y + s*axis.z, t*axis.y*axis.y + c,         t*axis.y*axis.z - s*axis.x,
                        t*axis.x*axis.z - s*axis.y, t*axis.y*axis.z + s*axis.x,  t*axis.z*axis.z + c
                    );
                }

                float3 vertex = input.positionOS;

                if (node.isTip == 1)
                {
                    float pinchFactor = saturate(1 - vertex.y);
                    vertex.xz *= pinchFactor;
                }

                float3 rotated = mul(rotationMatrix, vertex);
                float3 worldPos = _WorldOffset + node.currentPos + rotated;
                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);

                float3 worldNormal = normalize(mul(rotationMatrix, input.normalOS));
                output.normalWS = worldNormal;

                output.color = node.color;
                output.shadowCoord = TransformWorldToShadowCoord(worldPos);

                return output;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half3 normalWS = normalize(i.normalWS);
                half3 viewDirWS = normalize(_WorldSpaceCameraPos - i.positionWS);

                half3 baseColor = i.color.rgb * _BaseColor.rgb;
                half3 finalColor = 0;

                // === Main light ===
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = max(0, dot(normalWS, -lightDir));

                // Shadow attenuation
                half shadowAtten = MainLightRealtimeShadow(i.shadowCoord);
                finalColor += baseColor * mainLight.color * NdotL * shadowAtten;

                // === Additional lights ===
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint iLight = 0; iLight < additionalLightsCount; ++iLight)
                {
                    Light light = GetAdditionalLight(iLight, i.positionWS);
                    half3 lightDirAdd = normalize(light.direction);
                    half NdotLAdd = max(0, dot(normalWS, -lightDirAdd));
                    finalColor += baseColor * light.color * NdotLAdd * light.distanceAttenuation * light.shadowAttenuation;
                }

                return half4(finalColor, i.color.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
