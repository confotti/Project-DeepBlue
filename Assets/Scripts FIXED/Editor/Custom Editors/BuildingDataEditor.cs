using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingData))]
public class BuildingDataEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        var data = (BuildingData)target;

        if (data == null || data.Icon == null) return null;

        var texture = new Texture2D(width, height);
        EditorUtility.CopySerialized(data.Icon.texture, texture);

        return texture;
    }
}
