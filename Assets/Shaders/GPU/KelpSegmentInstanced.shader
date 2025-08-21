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

                // --- Get segment endpoints ---
                StalkNode nodeA = _StalkNodesBuffer[input.instanceID];
                StalkNode nodeB = _StalkNodesBuffer[input.instanceID + 1]; // next node

				float3 p0, p1;

				if (nodeA.isTip == 1)
				{
					p0 = _StalkNodesBuffer[input.instanceID - 1].currentPos; // base of tip
					p1 = nodeA.currentPos;                                    // tip position
				}
				else
				{
					p0 = nodeA.currentPos;
					p1 = _StalkNodesBuffer[input.instanceID + 1].currentPos;
				}

                float3 axis = p1 - p0;
                float len = length(axis);
                float3 dir = (len > 0.0001) ? axis / len : float3(0,1,0);
				float lenAdjusted = len * 1.1; 

                // --- Build rotation matrix from up -> dir ---
                float3 up = float3(0,1,0);
                float3 rotAxis = cross(up, dir);
                float angle = acos(saturate(dot(up, dir)));

                float s = sin(angle);
                float c = cos(angle);
                float t = 1 - c;

                float3x3 rotationMatrix;
                if (length(rotAxis) < 0.001)
                {
                    rotationMatrix = float3x3(1,0,0, 0,1,0, 0,0,1);
                }
                else
                {
                    rotAxis = normalize(rotAxis);
                    rotationMatrix = float3x3(
                        t*rotAxis.x*rotAxis.x + c,         t*rotAxis.x*rotAxis.y - s*rotAxis.z,  t*rotAxis.x*rotAxis.z + s*rotAxis.y,
                        t*rotAxis.x*rotAxis.y + s*rotAxis.z, t*rotAxis.y*rotAxis.y + c,         t*rotAxis.y*rotAxis.z - s*rotAxis.x,
                        t*rotAxis.x*rotAxis.z - s*rotAxis.y, t*rotAxis.y*rotAxis.z + s*rotAxis.x,  t*rotAxis.z*rotAxis.z + c
                    );
                }

                // --- Transform cube vertices ---
                float3 vertex = input.positionOS;

                // Scale Y to match segment length
                vertex.y *= len;

                bool isLastNode = (input.instanceID == _StalkNodesBuffer.Length - 1); 

                // --- Tip node: collapse vertices to create a triangle ---
                if (nodeA.isTip == 1 || isLastNode) 
                {
                    // t goes 0..1 along segment (0 = base of tip, 1 = tip position)
					float t = saturate(vertex.y); 

					// Determine local right/forward vectors to spread the vertices
					float3 right = normalize(cross(abs(dir.y) < 0.99 ? float3(0,1,0) : float3(1,0,0), dir));
					float3 forward = cross(dir, right);

					// Fully pinch vertices to zero at the tip
					float pinch = 1.0 - t; // 1 at base, 0 at tip
					float3 offset = vertex.x * pinch * right + vertex.z * pinch * forward;

					// Move vertices along the segment
					float3 worldPos = lerp(p0, p1, t) + offset + _WorldOffset;

					// Output
					output.positionWS = worldPos;
					output.positionCS = TransformWorldToHClip(worldPos);

					// Normal points along the segment
					output.normalWS = dir;

					output.color = nodeA.color;
					output.shadowCoord = TransformWorldToShadowCoord(worldPos);
                }
                else
                {
                    // Regular segment
                    float3 rotated = mul(rotationMatrix, vertex);
                    float3 worldPos = _WorldOffset + p0 + rotated;

                    output.positionWS = worldPos;
                    output.positionCS = TransformWorldToHClip(worldPos);

                    output.normalWS = normalize(mul(rotationMatrix, input.normalOS)); 

                    output.color = nodeA.color;
                    output.shadowCoord = TransformWorldToShadowCoord(worldPos);
                } 

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

                finalColor = saturate(finalColor);
                return half4(finalColor, i.color.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
} 