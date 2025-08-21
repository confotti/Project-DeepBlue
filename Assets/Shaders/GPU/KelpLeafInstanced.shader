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

            struct LeafSegment
            {
                float3 currentPos; float pad0;
                float3 previousPos; float pad1;
                float4 color;
            }; 

            struct LeafObject
			{
				float4 orientation;
				float3 bendAxis;    float bendAngle;
				int    stalkNodeIndex;
				float  angleAroundStem;
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
                uint instanceID   : SV_InstanceID;
            };

			struct appdata
			{
				uint vertexID : SV_VertexID; 
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR; 
			};

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float4 color      : COLOR;
                float4 shadowCoord: TEXCOORD2; 
            };

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

			float3 RotateAxisAngle(float3 v, float3 axis, float angle)
				{
					float s = sin(angle);
					float c = cos(angle);
					return v * c + cross(axis, v) * s + axis * dot(axis, v) * (1 - c); 
				} 

            Varyings vert(Attributes IN)
			{
				Varyings OUT; 

				uint leafID = IN.instanceID;                  // one draw instance per LEAF
				uint baseSeg = leafID * _LeafNodesPerLeaf;    // ROOT of this leaf's mini-rope

				// read per-leaf data
				LeafObject lo = _LeafObjectsBuffer[leafID];

				// read this leaf's ROOT segment for placement & color
				LeafSegment rootSeg = _LeafSegmentsBuffer[baseSeg]; 

				// object-space vertex & normal
				float3 v = IN.positionOS;
				float3 n = IN.normalOS;

				// 1) orient mesh along rope base direction (computed in compute and stored in lo.orientation)
				v = RotateByQuaternion(v, lo.orientation);
				n = normalize(RotateByQuaternion(n, lo.orientation));

				// 2) apply smooth bend about bendAxis with bendAngle (tip bends more)
				float t = saturate(v.y);                  // assumes +Y is leaf length in mesh
				float ang = lo.bendAngle * t * t;        // quadratic falloff
				v = RotateAxisAngle(v, lo.bendAxis, ang);
				n = normalize(RotateAxisAngle(n, lo.bendAxis, ang));

				// 3) radial rotation around stem (decorative spin)
				float ca = cos(lo.angleAroundStem), sa = sin(lo.angleAroundStem);
				float3x3 rotY = float3x3(
					ca, 0, -sa,
					 0, 1,   0,
					sa, 0,  ca
				);
				v = mul(rotY, v);
				n = normalize(mul(rotY, n));

				// 4) place in world using this leaf's ROOT segment
				float3 worldPos = _WorldOffset + rootSeg.currentPos + v;

				OUT.positionWS = worldPos;
				OUT.positionCS = TransformWorldToHClip(worldPos);
				OUT.normalWS = normalize(n); 
				OUT.color = _BaseColor; 
				OUT.shadowCoord= TransformWorldToShadowCoord(worldPos);
				return OUT;
			} 

            half4 frag(Varyings i) : SV_Target
			{
				half3 N = normalize(i.normalWS);

				// Ensure light hits both sides by flipping if needed 
				half3 N_forLighting = (dot(N, GetMainLight().direction) < 0) ? -N : N; 

				// Main directional light
				Light mainLight = GetMainLight();
				half3 L = normalize(mainLight.direction);
				half NdotL = max(0, dot(N, L));
				half shadowAtten = MainLightRealtimeShadow(i.shadowCoord);

				// Ambient from environment probe / spherical harmonics
				half3 ambient = SampleSH(N);

				// Base lighting
				half3 col = i.color.rgb * (ambient + mainLight.color * NdotL * shadowAtten); 

				// Additional lights
				uint addCount = GetAdditionalLightsCount();
				for (uint li = 0; li < addCount; ++li){
					Light l2 = GetAdditionalLight(li, i.positionWS);
					half3 L2 = normalize(l2.direction);
					col += i.color.rgb * l2.color * max(0, dot(N, L2)) * l2.distanceAttenuation * l2.shadowAttenuation;
				}

				return half4(saturate(col), i.color.a); 
			}

			ENDHLSL 
        }
    }

    FallBack "Hidden/InternalErrorShader"
} 