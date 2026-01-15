Shader "Hidden/CustomPass/TerrainMeshBlend_HDRP17"
{
    Properties
    {
        _BlendDistance ("Blend Distance (World Units)", Float) = 1.0
        _BlendSharpness ("Blend Sharpness", Float) = 2.0
        _TerrainUVScale ("Terrain UV Scale", Float) = 0.1
        _NormalBlendStrength ("Normal Blend Strength", Range(0,1)) = 1.0

        _TerrainAlbedo ("Terrain Albedo", 2D) = "white" {}
        _TerrainNormal ("Terrain Normal", 2D) = "bump" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "TerrainMeshBlend"
            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

            // Terrain textures
            TEXTURE2D(_TerrainAlbedo);
            TEXTURE2D(_TerrainNormal);
            SAMPLER(sampler_linear_clamp);

            float _BlendDistance;
            float _BlendSharpness;
            float _TerrainUVScale;
            float _NormalBlendStrength;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            // Fullscreen triangle
            Varyings Vert(Attributes v)
            {
                Varyings o;
                float2 pos = float2(
                    (v.vertexID == 2) ? 3.0 : -1.0,
                    (v.vertexID == 1) ? -3.0 : 1.0
                );
                o.positionCS = float4(pos, 0, 1);
                o.uv = pos * 0.5 + 0.5;
                return o;
            }

            // Linearize HDRP depth
            float LinearEyeDepth(float rawDepth)
            {
                return _ZBufferParams.x / (rawDepth * _ZBufferParams.y - _ZBufferParams.z);
            }

            // Reconstruct world position from HDRP depth
            float3 ReconstructWorldPosition(float2 uv)
            {
                float rawDepth = LOAD_TEXTURE2D_X(_CameraDepthTexture, uv * _ScreenSize.xy).r;
                if (rawDepth <= 0.00001)
                    return float3(0,0,0);

                float linearDepth = LinearEyeDepth(rawDepth);
                float2 ndc = uv * 2.0 - 1.0;
                float3 viewPos = float3(
                    ndc.x * linearDepth,
                    ndc.y * linearDepth,
                    -linearDepth
                );

                // HDRP 17: Use _CameraViewToWorld instead of unity_CameraToWorld
                float4 worldPos4 = mul(_CameraViewToWorld, float4(viewPos, 1.0));
                return worldPos4.xyz;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // Camera color (bound automatically)
                float4 sceneColor = LOAD_TEXTURE2D_X(_InputTexture, uv * _ScreenSize.xy);

                // World position
                float3 worldPos = ReconstructWorldPosition(uv);

                // Normal buffer
                NormalData normalData;
                DecodeFromNormalBuffer(uv, normalData);
                float3 meshNormalWS = normalize(normalData.normalWS);

                // Terrain UV
                float2 terrainUV = worldPos.xz * _TerrainUVScale;

                // Terrain color & normal
                float3 terrainColor = SAMPLE_TEXTURE2D(_TerrainAlbedo, sampler_linear_clamp, terrainUV).rgb;
                float3 terrainNormalTS = SAMPLE_TEXTURE2D(_TerrainNormal, sampler_linear_clamp, terrainUV).xyz * 2.0 - 1.0;

                // Y-up to world
                float3 terrainNormalWS = normalize(float3(terrainNormalTS.x, terrainNormalTS.z, terrainNormalTS.y));

                // Height-based blend
                float blend = saturate(1.0 - abs(worldPos.y) / _BlendDistance);
                blend = pow(blend, _BlendSharpness);

                // Albedo blend
                float3 blendedColor = lerp(sceneColor.rgb, terrainColor, blend);

                // Normal blend
                float3 blendedNormal = normalize(lerp(meshNormalWS, terrainNormalWS, blend * _NormalBlendStrength));

                // Optional subtle contrast from normal facing
                float facing = saturate(blendedNormal.y);
                blendedColor *= lerp(0.95, 1.05, facing);

                return float4(blendedColor, 1.0);
            }

            ENDHLSL
        }
    }
}