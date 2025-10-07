using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class TerrainBlendBinder : MonoBehaviour
{
    [Tooltip("Terrain to sample from")]
    public Terrain terrain;

    [Tooltip("Material that uses Hidden/TerrainBlend shader")]
    public Material blendMaterial;

    [Range(0f, 5f)]
    public float blendHeight = 0.25f; // vertical falloff distance for blending

    [Range(0f, 1f)]
    public float blendSharpness = 0.5f; // controls smoothstep softness

    [Range(0f, 2f)]
    public float heightOffset = 0f; // extra offset added after sampling terrain

    // If terrain doesn't expose a heightmap texture to GPU, we create one from GetHeights
    public bool ensureRuntimeHeightTexture = true;
    public int runtimeHeightTextureSize = 1024;

    void OnEnable()
    {
        BakeAndBind();
    }

    void OnValidate()
    {
        BakeAndBind();
    }

    void Update()
    {
        // In editor, keep updating while editing; in play mode you can disable if expensive
#if UNITY_EDITOR
        if (!Application.isPlaying) BakeAndBind();
#endif
    }

    void BakeAndBind()
    {
        if (terrain == null || blendMaterial == null) return;

        var td = terrain.terrainData;
        if (td == null) return;

        Texture heightTex = td.heightmapTexture;

        // Some Unity versions don't expose a GPU heightmap; create one from GetHeights if requested
        if (heightTex == null && ensureRuntimeHeightTexture)
        {
            heightTex = CreateRuntimeHeightTexture(td, runtimeHeightTextureSize);
        }

        // Terrain space params
        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = td.size;

        blendMaterial.SetTexture("_Heightmap", heightTex);
        blendMaterial.SetVector("_TerrainPos", new Vector4(terrainPos.x, terrainPos.y, terrainPos.z, 0));
        blendMaterial.SetVector("_TerrainScale", new Vector4(terrainSize.x, terrainSize.y, terrainSize.z, 0));

        blendMaterial.SetFloat("_BlendHeight", blendHeight);
        blendMaterial.SetFloat("_BlendSharpness", blendSharpness);
        blendMaterial.SetFloat("_HeightOffset", heightOffset);

        // ensure the material knows heightmap texture size info (some platforms auto-set _Heightmap_TexelSize)
        if (heightTex != null)
        {
            blendMaterial.SetVector("_Heightmap_TexelSize", new Vector4(1.0f / heightTex.width, 1.0f / heightTex.height, heightTex.width, heightTex.height));
        }
    }

    Texture2D CreateRuntimeHeightTexture(TerrainData td, int resolution)
    {
        resolution = Mathf.Clamp(resolution, 16, 4096);
        float[,] heights = td.GetHeights(0, 0, td.heightmapResolution, td.heightmapResolution);

        // Create a square texture of given resolution and sample heights using bilinear
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RFloat, false, true);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float rx = (float)td.heightmapResolution / resolution;
        float ry = (float)td.heightmapResolution / resolution;

        Color[] pixels = new Color[resolution * resolution];
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // sample heights with bilinear
                float sx = x * rx;
                float sy = y * ry;
                int ix = Mathf.FloorToInt(sx);
                int iy = Mathf.FloorToInt(sy);
                float fx = sx - ix;
                float fy = sy - iy;

                // clamp
                int ix1 = Mathf.Min(ix + 1, td.heightmapResolution - 1);
                int iy1 = Mathf.Min(iy + 1, td.heightmapResolution - 1);
                ix = Mathf.Clamp(ix, 0, td.heightmapResolution - 1);
                iy = Mathf.Clamp(iy, 0, td.heightmapResolution - 1);

                float h00 = heights[iy, ix];
                float h10 = heights[iy, ix1];
                float h01 = heights[iy1, ix];
                float h11 = heights[iy1, ix1];

                float hx0 = Mathf.Lerp(h00, h10, fx);
                float hx1 = Mathf.Lerp(h01, h11, fx);
                float h = Mathf.Lerp(hx0, hx1, fy);

                pixels[y * resolution + x] = new Color(h, 0, 0, 0);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.name = "Runtime_TerrainHeight_" + td.name;
#if UNITY_EDITOR
        tex.hideFlags = UnityEngine.HideFlags.DontSave;
#endif
        return tex;
    }
}