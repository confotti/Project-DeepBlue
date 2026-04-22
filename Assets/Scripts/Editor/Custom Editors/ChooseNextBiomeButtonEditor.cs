using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChooseNextBiomeButton))]
public class ChooseNextBiomeButtonEditor : Editor
{
    SerializedProperty biomePortProp;
    SerializedProperty indexProp;

    void OnEnable()
    {
        biomePortProp = serializedObject.FindProperty("_biomePort");
        indexProp = serializedObject.FindProperty("_nextBiomeIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(biomePortProp);

        indexProp.intValue = BiomeEditorUtils.DrawBiomeDropdown(
            biomePortProp.objectReferenceValue,
            indexProp.intValue
        );

        serializedObject.ApplyModifiedProperties();
    }
}