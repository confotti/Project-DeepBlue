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
        if (SaveLoad.currentSavedata != null) SaveLoad.currentSavedata.playerInventory = new InventorySaveData(primaryInventorySystem);
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
            OnPlayerInventoryDisplayRequested?.Invoke(primaryInventorySystem, playerHotbarSize);
        }
    }

    protected override void LoadInventory(SaveData data)
    {
        if (data.playerInventory.invSystem != null)
        {
            primaryInventorySystem = data.playerInventory.invSystem;
            
            OnPlayerInventoryChanged?.Invoke();
        }
    }

    public bool AddToInventory(InventoryItemData data, int amount, out int amountRemaining, bool spawnItemOnFail = false)
    {

        if (primaryInventorySystem.AddToInventory(data, amount, out int remainingAmount))
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
        //TODO: Have to create RemoveItemFromInventory() function in InventorySystem. 
    }

    public void SaveInventory()
    {
        SaveLoad.currentSavedata.playerInventory = new InventorySaveData(primaryInventorySystem);
    }
        
}
