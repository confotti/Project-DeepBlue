using SaveLoadSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public abstract class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    [SerializeField] protected InventorySystem inventorySystem;

    public InventorySystem InventorySystem => inventorySystem;

    public static UnityAction<InventorySystem, int> OnDynamicInventoryDisplayRequested; //InvSystem to display, amount to offset display by

    //Updates the UI if we change anything in the holder. 
    private void OnValidate()
    {
        /*
        foreach (var slot in inventorySystem.InventorySlots)
        {
            //inventorySystem.OnInventorySlotChanged?.Invoke(slot);
        }
        */
    }

    protected virtual void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
    }

    protected virtual void OnDestroy()
    {
        if (inventorySystem != null) inventorySystem.OnDestroy();
    }

    protected abstract void LoadInventory(SaveData data);

    protected abstract void SaveInventory();

    protected virtual ItemStackSaveData[] InventoryToSaveData()
    {
        var items = new ItemStackSaveData[InventorySystem.InventorySize];
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].amount <= 0)
            {
                items[i] = new ItemStackSaveData()
                {
                    amount = -1, 
                    itemId = -1
                };
            }
            else
            {
                items[i] = new ItemStackSaveData()
                {
                    amount = inventorySystem.InventorySlots[i].StackSize,
                    itemId = inventorySystem.InventorySlots[i].ItemData.ID
                };
            }
        }
        return items;
    }

    protected void LoadFromSaveData(InventorySaveData data)
    {
        // Clear existing items
        inventorySystem.ClearInventory();

        var db = Resources.Load<ItemDatabase>("Item Database");

        // Load items
        for (int i = 0; i < data.slots.Length; i++)
        {
            if(data.slots[i].amount > 0)
            {
                inventorySystem.InventorySlots[i].UpdateInventorySlot(db.GetItem(data.slots[i].itemId),
                    data.slots[i].amount);
            }
        }
    }
}


