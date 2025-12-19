using SaveLoadSystem;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder
{
    public static UnityAction<InventorySystem, int> OnPlayerInventoryDisplayRequested;

    [SerializeField] private int playerHotbarSize = 10;
    public static UnityAction OnPlayerInventoryChanged;

    private void Start()
    {
        SaveLoad.currentSavedata = new SaveData();
    }

    void OnEnable()
    {
        SaveLoad.OnSaveGame += SaveInventory;
        SaveLoad.OnLoadGame += LoadInventory;
    }

    void OnDisable()
    {
        SaveLoad.OnSaveGame -= SaveInventory;
        SaveLoad.OnLoadGame -= LoadInventory;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            OnPlayerInventoryDisplayRequested?.Invoke(inventorySystem, playerHotbarSize);
        }
    }

    public bool AddToInventory(InventoryItemData data, int amount, out int amountRemaining, bool spawnItemOnFail = false)
    {

        if (inventorySystem.AddToInventory(data, amount, out int remainingAmount))
        {
            amountRemaining = 0;
            return true;
        }

        if (spawnItemOnFail)
        {
            //TODO: Drop from the player the remainingAmount here probably, 
            //but depends on how we want to handle trying to pick-up items with full inventory. 
        }

        amountRemaining = remainingAmount;
        return false;
    }

    public void RemoveItemFromInventory(InventoryItemData itemData, int amount)
    {
        inventorySystem.RemoveFromInventory(itemData, amount);
    }
    
    public void RemoveItemFromInventory(ItemCost cost)
    {
        inventorySystem.RemoveFromInventory(cost.ItemRequired, cost.AmountRequired);
    }

    protected override void LoadInventory(SaveData data)
    {
        if (data.playerInventory.slots != null)
        {
            LoadFromSaveData(data.playerInventory);

            OnPlayerInventoryChanged?.Invoke();
        }
    }

    protected override void SaveInventory()
    {
        SaveLoad.currentSavedata.playerInventory = new InventorySaveData(InventoryToSaveData());

    }
}
