using System;
using UnityEngine;
namespace SaveLoadSystem
{
    [System.Serializable]
    public class SaveData
    {
        public SerializableDictionary<string, InventorySaveData> chestDictionary;

        public InventorySaveData playerInventory;

        public SaveData()
        {
            chestDictionary = new SerializableDictionary<string, InventorySaveData>();
            playerInventory = new InventorySaveData();
        }
    }
}

[Serializable]
public struct InventorySaveData
{
    public ItemStackSaveData[] slots;
    public Vector3 position;
    public Quaternion rotation;
    public bool childOfSub;

    public InventorySaveData(ItemStackSaveData[] itemStackSaveData, Vector3 position, Quaternion rotation, bool childOfSub = true)
    {
        slots = itemStackSaveData;
        this.position = position;
        this.rotation = rotation;
        this.childOfSub = childOfSub;
    }

    public InventorySaveData(ItemStackSaveData[] itemStackSaveData)
    {
        slots = itemStackSaveData;
        position = Vector3.zero;
        rotation = Quaternion.identity;
        childOfSub = false;
    }
}

[Serializable]
public struct ItemStackSaveData
{
    public int itemId;
    public int amount;
}