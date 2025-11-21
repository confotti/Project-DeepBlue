#ifndef KELP_BUFFERS_INCLUDED
#define KELP_BUFFERS_INCLUDED
#define UNITY_DOTS_INSTANCING_ENABLED 

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

// Buffers
StructuredBuffer<StalkNode> _StalkNodesBuffer;
StructuredBuffer<float3> initialRootPositions;

// Control from C# (0 = normal object, 1 = kelp instancing)
float _Kelp_UseInstance = 0.0;

// --- Rotation from 'from' to 'to' ---
float3x3 RotationFromTo(float3 from, float3 to)
{
    from = normalize(from);
    to   = normalize(to);
    float c = dot(from, to);

    if (c > 0.9998)
        return float3x3(1,0,0, 0,1,0, 0,0,1);

    if (c < -0.9998)
    {
        float3 axis = normalize(abs(from.x) > 0.1 ? float3(0,1,0) : float3(1,0,0));
        float3 ortho = normalize(cross(from, axis));
        return float3x3(-1,0,0, 0,-1,0, 0,0,-1);
    }

    float3 v = cross(from, to);
    float s = length(v);
    float k = (1.0 - c) / (s * s);

    return float3x3(
        c + k*v.x*v.x,      k*v.x*v.y - v.z,  k*v.x*v.z + v.y,
        k*v.x*v.y + v.z,    c + k*v.y*v.y,    k*v.y*v.z - v.x,
        k*v.x*v.z - v.y,    k*v.y*v.z + v.x,  c + k*v.z*v.z
    );
}

// --- Shader Graph-compatible vertex transform ---
void Kelp_VertexTransform_float(
    float3 positionOS,
    float3 normalOS,
    uint InstanceID,
    float3 worldOffset,
    out float3 positionWS,
    out float3 normalWS
)
{
    // Normal object rendering (not instanced)
    if (_Kelp_UseInstance < 0.5)
    {
        positionWS = TransformObjectToWorld(positionOS);
        normalWS   = TransformObjectToWorldDir(normalOS);
        return;
    }

    uint idx = (uint)InstanceID;

    if (_StalkNodesBuffer.Length == 0 || idx >= _StalkNodesBuffer.Length)
    {
        positionWS = TransformObjectToWorld(positionOS);
        normalWS   = TransformObjectToWorldDir(normalOS);
        return;
    }

    // --- Sample node ---
    StalkNode node = _StalkNodesBuffer[idx];
    float3 p0 = node.currentPos;
    float3 p1 = (idx + 1 < _StalkNodesBuffer.Length) ? _StalkNodesBuffer[idx + 1].currentPos : p0 + float3(0,1,0);

    if (node.isTip == 1 && idx > 0)
    {
        p0 = _StalkNodesBuffer[idx - 1].currentPos;
        p1 = node.currentPos;
    }

    float3 dir = normalize(p1 - p0);
    float3x3 rot = RotationFromTo(float3(0,1,0), dir);

    // Apply scale in object space
    float3 posLocal  = positionOS;

    // Apply segment rotation
    float3 posRot    = mul(rot, posLocal);

    // Convert into world position using your kelp node positions
    positionWS = posRot + p0 + worldOffset;

    // Normal
    float3 norLocal = normalOS;
    float3 norRot   = mul(rot, norLocal);
    normalWS        = normalize(norRot); 
}

#endif 