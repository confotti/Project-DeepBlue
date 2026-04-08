using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

//[CreateAssetMenu(menuName = "Inventory System/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<InventoryItemData> itemDatabase;

#if UNITY_EDITOR
    [ContextMenu("Set IDs")]
    public void SetItemIDs()
    {
        itemDatabase = new List<InventoryItemData>();

        //var foundItems = Resources.LoadAll<InventoryItemData>("Scriptable Objects/Items").OrderBy(i => i.ID).ToList();
        
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(InventoryItemData).Name);
        
        // Load them
        InventoryItemData[] foundItems = guids
            .Select(guid => AssetDatabase.LoadAssetAtPath<InventoryItemData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(obj => obj != null)
            .ToArray();


        var hasIDInRange = foundItems.Where(I => I.ID != -1 && I.ID < foundItems.Length).OrderBy(I => I.ID).ToList();
        var hasIDNotInRange = foundItems.Where(I => I.ID != -1 && I.ID >= foundItems.Length).OrderBy(I => I.ID).ToList();
        var noID = foundItems.Where(I => I.ID <= -1).ToList();

        var index = 0;
        for (int i = 0; i < foundItems.Length; i++)
        {
            InventoryItemData itemToAdd;
            itemToAdd = hasIDInRange.Find(d => d.ID == i);

            if (itemToAdd != null)
            {
                itemDatabase.Add(itemToAdd);
            }
            else if (index < noID.Count())
            {
                noID[index].ID = i;
                itemToAdd = noID[index];
                index++;
                itemDatabase.Add(itemToAdd);
            }
            if (itemToAdd) EditorUtility.SetDirty(itemToAdd);
        }

        foreach (var item in hasIDNotInRange)
        {
            itemDatabase.Add(item);
            if (item) EditorUtility.SetDirty(item);
        }

        AssetDatabase.SaveAssetIfDirty(this);
    }
#endif

    public InventoryItemData GetItem(int id)
    {
        return itemDatabase.Find(i => i.ID == id);
    }
    
    public InventoryItemData GetItem(string displayName)
    {
        return itemDatabase.Find(i => i.DisplayName == displayName);
    }
}
