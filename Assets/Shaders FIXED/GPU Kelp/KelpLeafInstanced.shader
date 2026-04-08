Shader "Custom/HDRP/KelpLeafInstanced"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.2,0.7,0.2,1)
        _MainTex ("Leaf Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="HDRenderPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        // =========================================================
        // DEPTH PREPASS (REQUIRED FOR HDRP FOG)
        // =========================================================
        Pass
        {
            Name "DepthPrepass"
            Tags { "LightMode"="DepthPrepass" }

            ZWrite On
            ColorMask 0
            Cull Off

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
            StructuredBuffer<LeafObject>  _LeafObjectsBuffer;

            struct Attributes
            {
                float3 positionOS : POSITION;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
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
                v = RotateAxisAngle(v, lo.bendAxis, lo.bendAngle * bendFactor);

                float ca = cos(lo.angleAroundStem);
                float sa = sin(lo.angleAroundStem);
                float3x3 rotY = float3x3(ca,0,-sa, 0,1,0, sa,0,ca);
                v = mul(rotY, v);

                float3 worldPos = _WorldOffset + rootSeg.currentPos + v;
                OUT.positionCS = mul(unity_MatrixVP, float4(worldPos, 1.0));
                return OUT;
            }

            float fragDepth(Varyings IN) : SV_Depth
            {
                return IN.positionCS.z / IN.positionCS.w;
            }

            ENDHLSL
        }

        // =========================================================
        // FORWARD COLOR PASS
        // =========================================================
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="ForwardOnly" }

            Cull Off
            ZWrite On

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
            StructuredBuffer<LeafObject>  _LeafObjectsBuffer;

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                uint instanceID   : SV_InstanceID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
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

                v = RotateByQuaternion(v, lo.orientation);

                float leafLength = 5.0;
                float t = saturate(v.y / leafLength);
                float bendFactor = 4.0 * t * (1.0 - t);
                v = RotateAxisAngle(v, lo.bendAxis, lo.bendAngle * bendFactor);

                float ca = cos(lo.angleAroundStem);
                float sa = sin(lo.angleAroundStem);
                float3x3 rotY = float3x3(ca,0,-sa, 0,1,0, sa,0,ca);
                v = mul(rotY, v);

                float3 worldPos = _WorldOffset + rootSeg.currentPos + v;
                OUT.positionCS = mul(unity_MatrixVP, float4(worldPos, 1.0));
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings i) : SV_Target
            {
                return _MainTex.Sample(sampler_MainTex, i.uv) * _BaseColor;
            }

            ENDHLSL
        }
    }

    FallBack Off
}