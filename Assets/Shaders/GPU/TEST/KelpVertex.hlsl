#ifndef LEAF_BEND_FUNCTION_INCLUDED
#define LEAF_BEND_FUNCTION_INCLUDED

struct LeafSegment
{
    float3 currentPos;
    float3 previousPos;
    float4 color;
};

struct LeafObject
{
    float3 bendAxis;
    float bendAngle;
    float4 orientation;
    int stalkNodeIndex;
    float angleAroundStem;
    float2 pad;
};

// GLOBAL BUFFERS (set from C#)
StructuredBuffer<LeafSegment> _LeafSegmentsBuffer;
StructuredBuffer<LeafObject> _LeafObjectsBuffer;

void LeafBendFunction_float_float(
    float3 PositionOS,
    float3 NormalOS,
    float InstanceID,
    float3 WorldOffset,
    float LeafNodesPerLeaf,
    out float3 outPos,
    out float3 outNormal
)
{
    int leafID = (int)InstanceID;
    int nodesPerLeaf = max(1, (int)LeafNodesPerLeaf);

    LeafObject leaf = _LeafObjectsBuffer[leafID];
    LeafSegment rootSegment = _LeafSegmentsBuffer[leaf.stalkNodeIndex];
    float3 rootPos = rootSegment.currentPos;

    float leafLength = 1.0;
    if (nodesPerLeaf > 1)
    {
        LeafSegment tipSegment = _LeafSegmentsBuffer[leaf.stalkNodeIndex + nodesPerLeaf - 1];
        leafLength = length(tipSegment.currentPos - rootPos);
    }

    float t = saturate(PositionOS.y / leafLength);
    float bendFactor = 4.0 * t * (1.0 - t);

    float angle = leaf.bendAngle * bendFactor;
    float s = sin(angle);
    float c = cos(angle);

    float3 v = PositionOS;
    v = v * c + cross(leaf.bendAxis, v) * s + leaf.bendAxis * dot(leaf.bendAxis, v) * (1 - c);

    outPos = WorldOffset + rootPos + v;
    outNormal = NormalOS;
}

#endif