using System;
using UnityEngine;

[System.Serializable]
[ExecuteInEditMode]
public class UniqueID : MonoBehaviour
{
    [InspectorReadOnly, SerializeField] private string id = Guid.NewGuid().ToString();
    [SerializeField] private static SerializableDictionary<string, GameObject> idDatabase = 
        new SerializableDictionary<string, GameObject>();

    public string ID => id;

    private void OnValidate()
    {
        if (idDatabase.ContainsKey(id)) Generate();
        else idDatabase.Add(id, gameObject);
    }

    private void OnDestroy()
    {
        if(idDatabase.ContainsKey(id)) idDatabase.Remove(id);
    }

    private void Generate()
    {

    }
}
