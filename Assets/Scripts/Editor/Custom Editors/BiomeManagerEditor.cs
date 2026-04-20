using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BiomeManager))]
public class BiomeManagerEditor : Editor
{
    SerializedProperty indexProp;
    SerializedProperty biomesProp;

    void OnEnable()
    {
        biomesProp = serializedObject.FindProperty("Biomes");
        indexProp = serializedObject.FindProperty("nextBiomeIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(biomesProp, true);

        if (biomesProp.arraySize > 0)
        {
            string[] options = new string[biomesProp.arraySize];

            for (int i = 0; i < biomesProp.arraySize; i++)
            {
                var element = biomesProp.GetArrayElementAtIndex(i);
                var sceneName = element.FindPropertyRelative("_sceneName");
                options[i] = sceneName.stringValue;
            }

            indexProp.intValue = EditorGUILayout.Popup(
                "Next Biome",
                indexProp.intValue,
                options
            );
        }
        else
        {
            indexProp.intValue = -1;
        }

        serializedObject.ApplyModifiedProperties();
    }
}