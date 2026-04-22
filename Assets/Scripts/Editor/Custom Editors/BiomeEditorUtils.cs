using UnityEditor;
using UnityEngine;

public static class BiomeEditorUtils
{
    public static int DrawBiomeDropdown(Object biomePortObj, int currentIndex)
    {
        if (biomePortObj == null)
        {
            EditorGUILayout.HelpBox("Assign a BiomePort.", MessageType.Info);
            return -1;
        }

        SerializedObject portSO = new SerializedObject(biomePortObj);
        SerializedProperty biomesProp = portSO.FindProperty("Biomes");

        if (biomesProp != null && biomesProp.arraySize > 0)
        {
            string[] options = new string[biomesProp.arraySize];

            for (int i = 0; i < biomesProp.arraySize; i++)
            {
                var element = biomesProp.GetArrayElementAtIndex(i);
                var sceneName = element.FindPropertyRelative("_sceneName");
                options[i] = sceneName.stringValue;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, biomesProp.arraySize - 1);

            return EditorGUILayout.Popup("Next Biome", currentIndex, options);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Popup("Next Biome", 0, new[] { "No biomes available" });
            EditorGUI.EndDisabledGroup();

            return -1;
        }
    }
}