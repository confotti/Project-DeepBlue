Shader "Custom/KelpSegmentInstanced"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
		_MainTex ("Leaf Texture", 2D) = "white" {} 
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
			#pragma multi_compile_fog 
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

			TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex); 

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
				float2 uv         : TEXCOORD0; 
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : COLOR;
				float2 uv         : TEXCOORD3;
                float4 shadowCoord : TEXCOORD2;
				float fogFactor   : TEXCOORD4;  
            };

            Varyings vert(Attributes input)
			{
				Varyings output;

				StalkNode nodeA = _StalkNodesBuffer[input.instanceID];

				float3 p0, p1;

				if (nodeA.isTip == 1)
				{
					p0 = _StalkNodesBuffer[input.instanceID - 1].currentPos;
					p1 = nodeA.currentPos;
				}
				else
				{
					p0 = nodeA.currentPos;
					p1 = _StalkNodesBuffer[input.instanceID + 1].currentPos;
				}

				float3 axis = p1 - p0;
				float len = length(axis);
				float3 dir = (len > 0.0001) ? axis / len : float3(0,1,0);

				// Rotation matrix to align segment
				float3 up = float3(0,1,0);
				float3 rotAxis = cross(up, dir);
				float angle = acos(saturate(dot(up, dir)));

				float s = sin(angle);
				float c = cos(angle);
				float t = 1 - c;

				float3x3 rotationMatrix;
				if (length(rotAxis) < 0.001)
					rotationMatrix = float3x3(1,0,0, 0,1,0, 0,0,1);
				else
				{
					rotAxis = normalize(rotAxis);
					rotationMatrix = float3x3(
						t*rotAxis.x*rotAxis.x + c,         t*rotAxis.x*rotAxis.y - s*rotAxis.z,  t*rotAxis.x*rotAxis.z + s*rotAxis.y,
						t*rotAxis.x*rotAxis.y + s*rotAxis.z, t*rotAxis.y*rotAxis.y + c,         t*rotAxis.y*rotAxis.z - s*rotAxis.x,
						t*rotAxis.x*rotAxis.z - s*rotAxis.y, t*rotAxis.y*rotAxis.z + s*rotAxis.x,  t*rotAxis.z*rotAxis.z + c
					);
				}

				float3 vertex = input.positionOS;
				bool isLastNode = (input.instanceID == _StalkNodesBuffer.Length - 1);

				// --- Tip node ---
				if (nodeA.isTip == 1 || isLastNode)
				{
					float3 right = float3(1,0,0);
					float3 forward = float3(0,0,1);

					float t = saturate(vertex.y);
					float pinch = 1.0 - t;
					float3 offset = vertex.x * pinch * right + vertex.z * pinch * forward;

					float3 worldPos = lerp(p0, p1, t) + offset + _WorldOffset;
					output.positionWS = worldPos;
					output.positionCS = TransformWorldToHClip(worldPos);

					// Compute proper tip normal
					float3 tangent = normalize(p1 - p0);
					float3 normal = normalize(input.normalOS); // just rotate the vertex normal along rotationMatrix 
					output.normalWS = normal;

					output.color = _BaseColor; // consistent with leaf shader
					output.shadowCoord = TransformWorldToShadowCoord(worldPos);

					output.uv = input.uv;
					output.fogFactor = ComputeFogFactor(output.positionCS.z); 
				}
				// --- Regular segment ---
				else if (_StalkNodesBuffer[input.instanceID + 1].isTip != 1)
				{
					float3 rotated = mul(rotationMatrix, vertex);
					float3 worldPos = _WorldOffset + p0 + rotated;

					output.positionWS = worldPos;
					output.positionCS = TransformWorldToHClip(worldPos);

					// Rotate normal to world space
					output.normalWS = normalize(mul(rotationMatrix, input.normalOS));

					output.color = _BaseColor;
					output.shadowCoord = TransformWorldToShadowCoord(worldPos);

					output.uv = input.uv;
					output.fogFactor = ComputeFogFactor(output.positionCS.z);  
				}
				else
				{
					output.positionCS = float4(0,0,0,0);
					output.positionWS = float3(0,0,0);
					output.normalWS = float3(0,1,0);
					output.color = float4(0,0,0,0);
					output.uv = input.uv; 
					output.shadowCoord = float4(0,0,0,0);

					// Fog
					output.fogFactor = ComputeFogFactor(output.positionCS.z); 
				}

				return output;
			}

            half4 frag(Varyings i) : SV_Target
			{
				half3 N = normalize(i.normalWS);
				half3 N_forLighting = (dot(N, GetMainLight().direction) < 0) ? -N : N;

				Light mainLight = GetMainLight();
				half3 L = normalize(mainLight.direction);
				half NdotL = max(0, dot(N_forLighting, L));
				half shadowAtten = MainLightRealtimeShadow(i.shadowCoord);

				// Ambient via spherical harmonics 
				half3 ambient = SampleSH(N);
				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv); 

				// Base color of stalk
				half3 col = i.color.rgb * texColor.rgb * (ambient + mainLight.color.rgb * NdotL * shadowAtten); 

				uint addCount = GetAdditionalLightsCount();
				for (uint li = 0; li < addCount; ++li)
				{
					Light l2 = GetAdditionalLight(li, i.positionWS);
					half3 L2 = normalize(l2.direction);
					col += i.color.rgb * texColor.rgb * l2.color.rgb * max(0, dot(N_forLighting, L2)) * l2.distanceAttenuation * l2.shadowAttenuation;
				}

				half4 finalColor = half4(saturate(col), texColor.a * i.color.a);

				// Apply fog properly
				finalColor.rgb = MixFog(finalColor.rgb, i.fogFactor);

				return finalColor;
			}

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}