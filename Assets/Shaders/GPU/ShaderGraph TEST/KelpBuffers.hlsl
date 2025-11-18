#ifndef KELP_BUFFERS_INCLUDED
#define KELP_BUFFERS_INCLUDED

// --- StalkNode buffer ---
// Matches your C# struct layout (padding included for correct stride)
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
StructuredBuffer<float3> initialRootPositions; // optional roots access

// --- Helper: Rotation matrix from 'from' to 'to' ---
float3x3 RotationFromTo(float3 from, float3 to)
{
    from = normalize(from);
    to = normalize(to);
    float c = saturate(dot(from, to));

    if (c > 0.9999)
        return float3x3(1,0,0, 0,1,0, 0,0,1);

    float3 axis = cross(from, to);
    float s = length(axis);
    float x = axis.x, y = axis.y, z = axis.z;
    float K = (1.0 - c) / (s*s);

    float3x3 R;
    R[0][0] = c + K*x*x;
    R[0][1] = K*x*y - z;
    R[0][2] = K*x*z + y;
    R[1][0] = K*x*y + z;
    R[1][1] = c + K*y*y;
    R[1][2] = K*y*z - x;
    R[2][0] = K*x*z - y;
    R[2][1] = K*y*z + x;
    R[2][2] = c + K*z*z;
    return R;
}

// --- Shader Graph-compatible vertex transform ---
// Note: instanceID must be passed from Shader Graph
void Kelp_VertexTransform_float(
    float3 positionOS,
    float3 normalOS,
    uint instanceID,
    float3 worldOffset,
    out float3 positionWS,
    out float3 normalWS
)
{
    // Safe access to object-to-world matrix
    float4x4 objToWorld = GetObjectToWorldMatrix();

    uint idx = instanceID;

    // Clamp to buffer length
    if (idx >= _StalkNodesBuffer.Length)
    {
        // fallback to standard transform
        positionWS = mul(objToWorld, float4(positionOS,1)).xyz + worldOffset;
        normalWS = normalize(mul((float3x3)objToWorld, normalOS));
        return;
    }

    // Sample current node
    StalkNode node = _StalkNodesBuffer[idx];
    float3 p0 = node.currentPos;
    float3 p1 = (idx + 1 < _StalkNodesBuffer.Length) ? _StalkNodesBuffer[idx+1].currentPos : p0 + float3(0,1,0);

    // If this is a tip, adjust positions
    if (node.isTip == 1 && idx > 0)
        p0 = _StalkNodesBuffer[idx-1].currentPos;

    float3 dir = normalize(p1 - p0);

    // Build rotation from up to direction
    float3x3 rot = RotationFromTo(float3(0,1,0), dir);

    // Transform position and normal
    positionWS = p0 + mul(rot, positionOS) + worldOffset;
    normalWS = normalize(mul(rot, normalOS));
}

#endif