Shader "Hidden/TerrainBlend"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Heightmap ("Terrain Heightmap", 2D) = "white" {}
        _TerrainPos ("Terrain Pos", Vector) = (0,0,0,0)
        _TerrainScale ("Terrain Scale", Vector) = (1,1,1,0)
        _BlendHeight ("Blend Height (meters)", Float) = 0.25
        _BlendSharpness ("Blend Sharpness", Float) = 0.5
        _HeightOffset ("Height Offset", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="HDRenderPipeline" }
        LOD 200

        Pass
        {
            Name "FORWARD"
            Cull Back
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #pragma target 4.5       // ✅ allow texture sampling in vertex shader
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Heightmap;
            float4 _TerrainPos;
            float4 _TerrainScale;
            float _BlendHeight;
            float _BlendSharpness;
            float _HeightOffset;
            float4 _Heightmap_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float blendMask : TEXCOORD3;
            };

            // Safe vertex-sampler version of height lookup
            float SampleHeight(float2 uv)
            {
                uv = saturate(uv);
                // Use tex2Dlod for vertex shader sampling
                return tex2Dlod(_Heightmap, float4(uv, 0, 0)).r;
            }

            float3 SampleTerrainNormal(float2 uv)
            {
                float2 ts = _Heightmap_TexelSize.xy;
                float hL = SampleHeight(uv + float2(-ts.x, 0));
                float hR = SampleHeight(uv + float2(ts.x, 0));
                float hD = SampleHeight(uv + float2(0, -ts.y));
                float hU = SampleHeight(uv + float2(0, ts.y));

                float worldHL = hL * _TerrainScale.y + _TerrainPos.y;
                float worldHR = hR * _TerrainScale.y + _TerrainPos.y;
                float worldHD = hD * _TerrainScale.y + _TerrainPos.y;
                float worldHU = hU * _TerrainScale.y + _TerrainPos.y;

                float dx = (worldHR - worldHL) / (_TerrainScale.x * 2.0 * _Heightmap_TexelSize.x);
                float dz = (worldHU - worldHD) / (_TerrainScale.z * 2.0 * _Heightmap_TexelSize.y);

                float3 n = normalize(float3(-dx, 1.0, -dz));
                return n;
            }

            v2f vert(appdata v)
            {
                v2f o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));

                float2 terrainUV;
                terrainUV.x = (worldPos.x - _TerrainPos.x) / _TerrainScale.x;
                terrainUV.y = (worldPos.z - _TerrainPos.z) / _TerrainScale.z;

                float hNorm = SampleHeight(terrainUV);
                float terrainWorldH = hNorm * _TerrainScale.y + _TerrainPos.y;

                float displacement = (terrainWorldH - worldPos.y) + _HeightOffset;

                float edge = saturate(abs(displacement) / max(1e-6, _BlendHeight));
                float blend = 1.0 - smoothstep(0.0, 1.0, pow(edge, 1.0 + _BlendSharpness * 4.0));

                float3 displacedWorld = worldPos;
                displacedWorld.y += displacement * blend;

                float3 terrainNormal = SampleTerrainNormal(terrainUV);
                float3 finalNormalWorld = normalize(lerp(worldNormal, terrainNormal, blend));

                o.worldPos = displacedWorld;
                o.worldNormal = finalNormalWorld;
                o.uv = v.uv;
                o.blendMask = blend;
                o.pos = UnityWorldToClipPos(displacedWorld);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 albedo = tex2D(_MainTex, i.uv);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float diff = saturate(dot(i.worldNormal, -lightDir));
                float3 col = albedo.rgb * (0.18 + 0.82 * diff);
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    FallBack Off
}