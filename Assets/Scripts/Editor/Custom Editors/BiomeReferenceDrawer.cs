using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(BiomeReference))]
public class BiomeReferenceDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Add extra space for help box
        return EditorGUIUtility.singleLineHeight * 2.5f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty sceneNameProp = property.FindPropertyRelative("_sceneName");

        // Get all scenes in build settings
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
            .ToArray();

        // Current index
        int currentIndex = System.Array.IndexOf(scenes, sceneNameProp.stringValue);
        if (currentIndex < 0) currentIndex = 0;

        // Layout rects
        Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect helpBoxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        // Dropdown
        int newIndex = EditorGUI.Popup(dropdownRect, label.text, currentIndex, scenes);

        if (newIndex >= 0 && newIndex < scenes.Length)
        {
            sceneNameProp.stringValue = scenes[newIndex];
        }

        // Help message
        if (!scenes.Contains(sceneNameProp.stringValue))
        {
            EditorGUI.HelpBox(helpBoxRect, "Selected scene is not in Build Settings!", MessageType.Warning);
        }
        else
        {
            EditorGUI.HelpBox(helpBoxRect, "If a scene doesn't show up, it's not in the Build Settings.", MessageType.Info);
        }
    }
}