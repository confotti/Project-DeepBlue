struct Attributes
{
    float3 positionOS : POSITION;
    float3 normalOS   : NORMAL;
    uint instanceID   : SV_InstanceID;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 normalWS   : TEXCOORD0;
};

Varyings Vert(Attributes IN)
{
    Varyings OUT;
    float3 posWS;
    float3 norWS;

    // Call your existing ShaderGraph function:
    Kelp_VertexTransform_float(
        IN.positionOS,
        IN.normalOS,
        (float)IN.instanceID,
        _WorldOffset,
        posWS,
        norWS
    );

    OUT.positionCS = TransformWorldToHClip(posWS);
    OUT.normalWS = norWS;
    return OUT;
}