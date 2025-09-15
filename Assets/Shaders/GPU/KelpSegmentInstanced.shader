Shader "Custom/HDRP/KelpSegmentInstanced"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0,1,0,1)
        _MainTex ("Leaf Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="HDRenderPipeline" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="Forward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 5.0

            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPassForward.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoop.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            StructuredBuffer<StalkNode>
            {
                float3 currentPos;
                float pad0;
                float3 previousPos;
                float pad1;
                float3 direction;
                float pad2;
                float4 color;
                float bendAmount;
                float3 pad3;
                int isTip;
                float3 pad4;
            } _StalkNodesBuffer;

            float4 _BaseColor;
            float3 _WorldOffset;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                uint instanceID : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float4 color : COLOR;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Fetch node data
                StalkNode node = _StalkNodesBuffer[IN.instanceID];

                float3 p0, p1;

                if(node.isTip == 1)
                {
                    p0 = _StalkNodesBuffer[IN.instanceID - 1].currentPos;
                    p1 = node.currentPos;
                }
                else
                {
                    p0 = node.currentPos;
                    p1 = _StalkNodesBuffer[IN.instanceID + 1].currentPos;
                }

                // Align segment
                float3 axis = p1 - p0;
                float len = length(axis);
                float3 dir = (len > 0.0001) ? axis / len : float3(0,1,0);

                float3 up = float3(0,1,0);
                float3 rotAxis = cross(up, dir);
                float angle = acos(saturate(dot(up, dir)));
                float s = sin(angle);
                float c = cos(angle);
                float t = 1 - c;

                float3x3 rotationMatrix;
                if(length(rotAxis) < 0.001)
                    rotationMatrix = float3x3(1,0,0, 0,1,0, 0,0,1);
                else
                {
                    rotAxis = normalize(rotAxis);
                    rotationMatrix = float3x3(
                        t*rotAxis.x*rotAxis.x + c, t*rotAxis.x*rotAxis.y - s*rotAxis.z, t*rotAxis.x*rotAxis.z + s*rotAxis.y,
                        t*rotAxis.x*rotAxis.y + s*rotAxis.z, t*rotAxis.y*rotAxis.y + c, t*rotAxis.y*rotAxis.z - s*rotAxis.x,
                        t*rotAxis.x*rotAxis.z - s*rotAxis.y, t*rotAxis.y*rotAxis.z + s*rotAxis.x, t*rotAxis.z*rotAxis.z + c
                    );
                }

                float3 vertex = IN.positionOS;

                // Tip node
                if(node.isTip == 1 || IN.instanceID == _StalkNodesBuffer.Length - 1)
                {
                    float3 right = float3(1,0,0);
                    float3 forward = float3(0,0,1);

                    float tLerp = saturate(vertex.y);
                    float pinch = 1.0 - tLerp;
                    float3 offset = vertex.x * pinch * right + vertex.z * pinch * forward;

                    float3 worldPos = lerp(p0, p1, tLerp) + offset + _WorldOffset;
                    OUT.positionWS = worldPos;
                    OUT.positionCS = mul(GetHDCamera().projectionMatrix, mul(GetHDCamera().worldToCameraMatrix, float4(worldPos,1)));
                    OUT.normalWS = normalize(IN.normalOS);
                    OUT.color = _BaseColor;
                    OUT.uv = IN.uv;
                }
                // Regular segment
                else
                {
                    float3 rotated = mul(rotationMatrix, vertex);
                    float3 worldPos = p0 + rotated + _WorldOffset;
                    OUT.positionWS = worldPos;
                    OUT.positionCS = mul(GetHDCamera().projectionMatrix, mul(GetHDCamera().worldToCameraMatrix, float4(worldPos,1)));
                    OUT.normalWS = normalize(mul(rotationMatrix, IN.normalOS));
                    OUT.color = _BaseColor;
                    OUT.uv = IN.uv;
                }

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 N = normalize(IN.normalWS);

                // HDRP main directional light
                Light mainLight = GetMainLight();
                half3 L = normalize(mainLight.direction);
                half NdotL = max(0, dot(N, L));

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 col = IN.color.rgb * texColor.rgb * mainLight.color.rgb * NdotL;

                return half4(saturate(col), texColor.a * IN.color.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}