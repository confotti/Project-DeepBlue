Shader "Custom/KelpSegmentShader"
{
    Properties
    {
        _Color ("Color", Color) = (0.3, 0.6, 0.3, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct appdata members segmentHeight,y,wave)
#pragma exclude_renderers d3d11
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                uint instanceID : SV_InstanceID; // <--- ✅ This is the keyfloat stalkHeight = Random.Range(3f, 7f);
float segmentHeight = stalkHeight / segmentsPerKelp;

for (int s = 0; s <= segmentsPerKelp; s++)
{
    float y = s * segmentHeight;
    float wave = Mathf.Sin(Time.time * 1.5f + s * 0.3f + k) * 0.2f;
    nodes[k * (segmentsPerKelp + 1) + s] = origin + new Vector3(wave, y, 0f);
}
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            struct SegmentData
            {
                float3 basePosition;
                float kelpID;
                float segmentIndex;
                float segmentCount;
                float padding;
            };

            StructuredBuffer<SegmentData> _InstanceBuffer;
            StructuredBuffer<float3> _NodeBuffer;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;

                SegmentData seg = _InstanceBuffer[v.instanceID];

                int kelpID = (int)seg.kelpID;
                int segmentIndex = (int)seg.segmentIndex;
                int segmentCount = (int)seg.segmentCount;

                int nodeBase = kelpID * (segmentCount + 1) + segmentIndex;
                float3 bottomNode = _NodeBuffer[nodeBase];
                float3 topNode = _NodeBuffer[nodeBase + 1];

                float t = saturate(v.vertex.y); // Assumes Y from 0 to 1 in mesh
                float3 worldPos = lerp(bottomNode, topNode, t);

                // Pinch top segment
                if (segmentIndex == segmentCount - 1 && t > 0.99)
                {
                    worldPos = topNode;
                }

                o.pos = UnityObjectToClipPos(float4(worldPos, 1.0));
                o.color = _Color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}