#ifndef KELP_BUFFERS_INCLUDED
#define KELP_BUFFERS_INCLUDED

#define UNITY_DOTS_INSTANCING_ENABLED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

// --- StalkNode buffer ---
struct StalkNode
{
    float3 currentPos;  float pad0;
    float3 previousPos; float pad1;
    float3 direction;   float pad2;
    float4 color;
    float  bendAmount;  float3 pad3;
    int    isTip;       float3 pad4;
};

StructuredBuffer<StalkNode> _StalkNodesBuffer;
float _Kelp_UseInstance = 0.0;

// --- Rotation helper ---
float3x3 RotationFromTo(float3 from, float3 to)
{
    from = normalize(from);
    to   = normalize(to);
    float c = dot(from, to);
    if (c > 0.9998) return float3x3(1,0,0, 0,1,0, 0,0,1);
    if (c < -0.9998)
    {
        float3 axis = normalize(abs(from.x) > 0.1 ? float3(0,1,0) : float3(1,0,0));
        float3 ortho = normalize(cross(from, axis));
        return float3x3(-1,0,0, 0,-1,0, 0,0,-1);
    }
    float3 v = cross(from, to);
    float s = length(v);
    float k = (1.0 - c)/(s*s);
    return float3x3(
        c + k*v.x*v.x, k*v.x*v.y - v.z, k*v.x*v.z + v.y,
        k*v.x*v.y + v.z, c + k*v.y*v.y, k*v.y*v.z - v.x,
        k*v.x*v.z - v.y, k*v.y*v.z + v.x, c + k*v.z*v.z
    );
}

// --- ShaderGraph vertex transform ---
void Kelp_VertexTransform_float(
    float3 positionOS_IN,
    float3 normalOS_IN,
    float InstanceIDParam,
    float3 worldOffset,
    out float3 positionOS,
    out float3 normalOS
)
{
    uint InstanceID = (uint)InstanceIDParam;

    if (_Kelp_UseInstance < 0.5 || _StalkNodesBuffer.Length == 0 || InstanceID >= _StalkNodesBuffer.Length)
    {
        positionOS = positionOS_IN;
        normalOS = normalOS_IN;
        return;
    }

    // Sample node
    StalkNode node = _StalkNodesBuffer[InstanceID];
    float3 p0 = node.currentPos;
    float3 p1 = (InstanceID + 1 < _StalkNodesBuffer.Length) ? 
                _StalkNodesBuffer[InstanceID+1].currentPos : 
                p0 + float3(0,1,0);

    if (node.isTip == 1 && InstanceID > 0)
    {
        p0 = _StalkNodesBuffer[InstanceID-1].currentPos;
        p1 = node.currentPos;
    }

    float3 dir = normalize(p1 - p0);
    float3x3 rot = RotationFromTo(float3(0,1,0), dir);

    float3 posWS = mul(rot, positionOS_IN) + p0 + worldOffset;
    float3 norWS = mul(rot, normalOS_IN);

    // Use SRP-safe matrices
    // Convert world → object space
    positionOS = mul(UNITY_MATRIX_I_M, float4(posWS, 1)).xyz;
    normalOS   = mul((float3x3)UNITY_MATRIX_I_M, norWS);
    normalOS   = normalize(normalOS);
}

#endif