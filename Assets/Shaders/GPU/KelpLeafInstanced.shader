Shader "Custom/KelpLeafInstanced"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2,0.7,0.2,1)
        _MainTex ("Leaf Texture", 2D) = "white" {}
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct LeafSegment
            {
                float3 currentPos; float pad0;
                float3 previousPos; float pad1;
                float4 color;
            }; 

            struct LeafObject
            {
                float4 orientation;
                float3 bendAxis; float bendAngle;
                int stalkNodeIndex;
                float angleAroundStem;
                float2 pad; 
            };

            StructuredBuffer<LeafSegment> _LeafSegmentsBuffer;
            StructuredBuffer<LeafObject> _LeafObjectsBuffer; 

            float3 _WorldOffset;
            float4 _BaseColor;
            int _LeafNodesPerLeaf; 

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD3;
                float4 shadowCoord: TEXCOORD2; 
            };

            float3 RotateByQuaternion(float3 v, float4 q)
            {
                float3 t = 2.0 * cross(q.xyz, v);
                return v + q.w * t + cross(q.xyz, t);
            }

            float3 RotateAxisAngle(float3 v, float3 axis, float angle)
            {
                float s = sin(angle);
                float c = cos(angle);
                return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1 - c); 
            } 

            Varyings vert(Attributes IN)
            {
                Varyings OUT; 

                uint leafID = IN.instanceID;
                uint baseSeg = leafID * _LeafNodesPerLeaf;

                LeafObject lo = _LeafObjectsBuffer[leafID];
                LeafSegment rootSeg = _LeafSegmentsBuffer[baseSeg];

                float3 v = IN.positionOS;
                float3 n = IN.normalOS;

                // Orient mesh along root direction
                v = RotateByQuaternion(v, lo.orientation);
                n = normalize(RotateByQuaternion(n, lo.orientation));

                // Bend toward tip
                float t = saturate(v.y);
                float ang = lo.bendAngle * t * t;
                v = RotateAxisAngle(v, lo.bendAxis, ang);
                n = normalize(RotateAxisAngle(n, lo.bendAxis, ang));

                // Radial rotation around stalk
                float ca = cos(lo.angleAroundStem), sa = sin(lo.angleAroundStem);
                float3x3 rotY = float3x3(
                    ca, 0, -sa,
                     0, 1,   0,
                    sa, 0,  ca
                );
                v = mul(rotY, v);
                n = normalize(mul(rotY, n));

                float3 worldPos = _WorldOffset + rootSeg.currentPos + v;

                OUT.positionWS = worldPos;
                OUT.positionCS = TransformWorldToHClip(worldPos);
                OUT.normalWS = normalize(n);
                OUT.color = _BaseColor;
                OUT.uv = IN.uv;
                OUT.shadowCoord = TransformWorldToShadowCoord(worldPos);

                return OUT;
            } 

            half4 frag(Varyings i) : SV_Target
            {
                half3 N = normalize(i.normalWS);

                // Flip normals if needed
                half3 N_forLighting = (dot(N, GetMainLight().direction) < 0) ? -N : N; 

                Light mainLight = GetMainLight();
                half3 L = normalize(mainLight.direction);
                half NdotL = max(0, dot(N, L));
                half shadowAtten = MainLightRealtimeShadow(i.shadowCoord);

                half3 ambient = SampleSH(N);

                // Sample leaf texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                half3 col = i.color.rgb * texColor.rgb * (ambient + mainLight.color * NdotL * shadowAtten);

                uint addCount = GetAdditionalLightsCount();
                for (uint li = 0; li < addCount; ++li)
                {
                    Light l2 = GetAdditionalLight(li, i.positionWS);
                    half3 L2 = normalize(l2.direction);
                    col += i.color.rgb * texColor.rgb * l2.color * max(0, dot(N, L2)) * l2.distanceAttenuation * l2.shadowAttenuation;
                }

                return half4(saturate(col), texColor.a * i.color.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}