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
            StructuredBuffer<StalkNode> _StalkNodesBuffer;
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
                // v' = v + 2.0 * cross(q.xyz, cross(q.xyz, v) + q.w * v)
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

                // read leaf and its parent stalk node
                LeafObject leaf = _LeafObjectsBuffer[input.instanceID];
                StalkNode stalk = _StalkNodesBuffer[leaf.stalkNodeIndex];

                // object-space vertex
                float3 v = input.positionOS;
                float3 n = input.normalOS;

                // --- geometric adjustments in leaf-local space --- 

                // 2) apply bending:
                //    We bend the leaf by rotating vertices around the leaf-local X axis
                //    The amount uses bendValue and the vertex's vertical (y) to vary along length.
                //    You can tune the 0.5 multiplier if desired.
                float bendStrength = leaf.bendValue * 0.8; // small overall scale
                // Use a smooth curve so base is stable and tip bends more:
                float t = saturate(v.y);             // 0 at base, 1 at tip
                float bendAngle = bendStrength * (t * t); // quadratic distribution
                v = RotateAroundLocalX(v, bendAngle);

                // 3) rotate the vertex by the leaf orientation quaternion (transforms from leaf-local to world orientation)
                v = RotateByQuaternion(v, leaf.orientation);
                n = normalize(RotateByQuaternion(n, leaf.orientation));

                // 4) translate into world: worldPos = worldOffset + stalk.currentPos + v
                float3 worldPos = _WorldOffset + stalk.currentPos + v;

                output.positionWS = worldPos;
                output.positionCS = TransformWorldToHClip(worldPos);
                output.normalWS = n;
                output.color = stalk.color * _BaseColor; // combine stalk node color with base color
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