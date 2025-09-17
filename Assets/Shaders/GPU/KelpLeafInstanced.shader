Shader "Custom/HDRP/KelpLeafInstanced"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2,0.7,0.2,1)
        _MainTex ("Leaf Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="HDRenderPipeline" }

        Pass
        {
            Name "UnlitInstanced"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Off
            ZWrite On
            Stencil
            {
                Ref 1
                Comp always
                Pass replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma target 5.0

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _BaseColor;
            float3 _WorldOffset;
            int _LeafNodesPerLeaf;
            float4x4 unity_ObjectToWorld;
            float4x4 unity_WorldToObject;
            float4x4 unity_MatrixVP;

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
                float2 uv         : TEXCOORD2;
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

                v = RotateByQuaternion(v, lo.orientation);
                n = normalize(RotateByQuaternion(n, lo.orientation));

                float leafLength = 5.0;
                float t = saturate(v.y / leafLength);      
                float bendFactor = 4.0 * t * (1.0 - t);
                float ang = lo.bendAngle * bendFactor;
                v = RotateAxisAngle(v, lo.bendAxis, ang);
                n = normalize(RotateAxisAngle(n, lo.bendAxis, ang)); 

                float ca = cos(lo.angleAroundStem), sa = sin(lo.angleAroundStem);
                float3x3 rotY = float3x3(ca,0,-sa, 0,1,0, sa,0,ca);
                v = mul(rotY, v);
                n = normalize(mul(rotY, n));

                float3 worldPos = _WorldOffset + rootSeg.currentPos + v;
                OUT.positionWS = worldPos;
                OUT.positionCS = mul(unity_MatrixVP, float4(worldPos, 1.0));
                OUT.normalWS = normalize(n);
                OUT.color = _BaseColor;
                OUT.uv = IN.uv;

                return OUT;
            } 

            half4 frag(Varyings i) : SV_Target
            {
                half4 texColor = _MainTex.Sample(sampler_MainTex, i.uv); 
                return texColor * i.color;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0
            Cull Off
            Stencil
            {
                Ref 1
                Comp always
                Pass replace
            }

            HLSLPROGRAM
            #pragma vertex vertDepth
            #pragma fragment fragDepth
            #pragma multi_compile_instancing
            #pragma target 5.0

            float3 _WorldOffset;
            int _LeafNodesPerLeaf;
            float4x4 unity_MatrixVP;

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

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID   : SV_InstanceID;
            };
            struct Varyings { float4 positionCS : SV_POSITION; };

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

            Varyings vertDepth(Attributes IN)
            {
                Varyings OUT; 
                uint leafID = IN.instanceID;
                uint baseSeg = leafID * _LeafNodesPerLeaf;
                LeafObject lo = _LeafObjectsBuffer[leafID];
                LeafSegment rootSeg = _LeafSegmentsBuffer[baseSeg];
                float3 v = IN.positionOS;
                v = RotateByQuaternion(v, lo.orientation);

                float leafLength = 5.0;
                float t = saturate(v.y / leafLength);      
                float bendFactor = 4.0 * t * (1.0 - t);
                float ang = lo.bendAngle * bendFactor;
                v = RotateAxisAngle(v, lo.bendAxis, ang);

                float ca = cos(lo.angleAroundStem), sa = sin(lo.angleAroundStem);
                float3x3 rotY = float3x3(ca,0,-sa, 0,1,0, sa,0,ca);
                v = mul(rotY, v);

                float3 worldPos = _WorldOffset + rootSeg.currentPos + v;
                OUT.positionCS = mul(unity_MatrixVP, float4(worldPos, 1.0));
                return OUT;
            }

            float fragDepth(Varyings IN) : SV_Depth { return IN.positionCS.z / IN.positionCS.w; }

            ENDHLSL
        }
    }

    FallBack Off
}