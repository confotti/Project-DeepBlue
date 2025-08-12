Shader "Custom/KelpLeafInstanced"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2,0.7,0.2,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
			Cull off 

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

            // -----------------------
            // CPU-side struct mirrors
            // -----------------------
            struct LeafNode
            {
                float3 currentPos;
				float padding0;
                float3 previousPos;
				float padding1; 
                float4 color;
            };

            struct LeafObject
            {
                float4 orientation;   // quaternion (x,y,z,w)
                float bendValue;
                int stalkNodeIndex;
                int padding;          // to align to 16 bytes
            };

            // -----------------------
            // Buffers & uniforms
            // -----------------------
            StructuredBuffer<LeafNode> _LeafNodesBuffer;
            StructuredBuffer<LeafObject> _LeafObjectsBuffer;

            float3 _WorldOffset;
            float4 _BaseColor;

            // -----------------------
            // Vertex attributes & varyings
            // -----------------------
            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 color      : COLOR;
                float4 shadowCoord: TEXCOORD2;
            };

            // -----------------------
            // Helpers
            // -----------------------
            // Rotate vector v by quaternion q (q.xyz, q.w)
            float3 RotateByQuaternion(float3 v, float4 q)
            {
                float3 t = 2.0 * cross(q.xyz, v);
                float3 result = v + q.w * t + cross(q.xyz, t);
                return result;
            }

            // Rotate around local X axis by angle (radians) — operate on a float3
            float3 RotateAroundLocalX(float3 p, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                // x stays the same
                float y = p.y * c - p.z * s;
                float z = p.y * s + p.z * c;
                return float3(p.x, y, z);
            }

            // -----------------------
            // Vertex shader
            // -----------------------
            Varyings vert(Attributes input)
            {
                Varyings output;

                // read leaf and leaf-node (position + color)
                uint iid = input.instanceID;
                LeafObject leafObj = _LeafObjectsBuffer[iid];
                LeafNode leafNode = _LeafNodesBuffer[iid];

                // object-space vertex
                float3 v = input.positionOS;
                float3 n = input.normalOS;

                // --- geometric adjustments in leaf-local space ---

                // Apply bend
				float bendStrength = leafObj.bendValue * 0.8; 
				float t = saturate(v.y);
				float bendAngle = bendStrength * (t * t);
				v = RotateAroundLocalX(v, bendAngle);

				// Rotate into leaf orientation space
				v = RotateByQuaternion(v, leafObj.orientation);
				n = normalize(RotateByQuaternion(n, leafObj.orientation)); 

				// Translate into world using precomputed leaf position
				float3 worldPos = _WorldOffset + leafNode.currentPos + v;

				output.positionWS = worldPos;
				output.positionCS = TransformWorldToHClip(worldPos);
				output.normalWS = n;
				output.color = leafNode.color * _BaseColor;
				output.shadowCoord = TransformWorldToShadowCoord(worldPos);

                return output;
            }

            // -----------------------
            // Fragment shader
            // -----------------------
            half4 frag(Varyings i) : SV_Target
            {
                half3 normalWS = normalize(i.normalWS);
                half3 viewDirWS = normalize(_WorldSpaceCameraPos - i.positionWS);

                half3 baseColor = i.color.rgb;
                half3 finalColor = 0;

                // Main directional light
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = max(0, dot(normalWS, -lightDir));

                half shadowAtten = MainLightRealtimeShadow(i.shadowCoord);
                finalColor += baseColor * mainLight.color * NdotL * shadowAtten;

                // Additional lights (forward path)
                uint additionalLightsCount = GetAdditionalLightsCount();
                for (uint li = 0; li < additionalLightsCount; ++li)
                {
                    Light light = GetAdditionalLight(li, i.positionWS);
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