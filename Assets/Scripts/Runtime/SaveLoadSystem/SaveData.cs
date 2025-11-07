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
