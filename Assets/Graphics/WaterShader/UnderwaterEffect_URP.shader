Shader "Paro222/UnderwaterEffect_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _color ("Color", Color) = (0, 0.4, 0.6, 1)
        _dis ("Distance", Float) = 10
        _alpha ("Alpha", Range(0,1)) = 0.5
        _refraction ("Refraction", Float) = 0.1
        _normalUV ("Normal UV", Vector) = (1,1,0.2,0.1)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "UnderwaterURP"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _MainTex_ST;
            float4 _normalUV;
            float4 _color;
            float _dis;
            float _alpha;
            float _refraction;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half remap(half x, half t1, half t2, half s1, half s2)
            {
                return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
            }

            half4 Frag(Varyings IN) : SV_Target 
            {
                float2 uvOffset = _normalUV.xy * IN.uv + _normalUV.zw * _Time.y; 
				float3 normalmap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uvOffset));
				float2 offsetUV = IN.uv + normalmap.xy * _refraction * 0.01;

				float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, offsetUV).r;
				float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
				float depth = saturate(smoothstep(0, abs(_dis) * 0.01, linearDepth) + _alpha);

				float4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, offsetUV); 

				// DEBUG: output baseColor only
				return baseColor;

				// DEBUG: output depth as grayscale
				//return half4(depth, depth, depth, 1);

				// DEBUG: output normalmap xy as color
				//return half4(normalmap.xy * 0.5 + 0.5, 0, 1); 
            }
            ENDHLSL
        }
    }
    FallBack Off 
}