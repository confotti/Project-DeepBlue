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
        foreach (var slot in inventorySystem.InventorySlots)
        {
            inventorySystem.OnInventorySlotChanged?.Invoke(slot);
        }
    }

    protected virtual void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
    }

    protected abstract void LoadInventory(SaveData data);
}

[Serializable]
public struct InventorySaveData
{
    public InventorySystem invSystem;
    public Vector3 position;
    public Quaternion rotation;
    public bool childOfSub;

    public InventorySaveData(InventorySystem invSystem, Vector3 position, Quaternion rotation, bool childOfSub = true)
    {
        this.invSystem = invSystem;
        this.position = position;
        this.rotation = rotation;
        this.childOfSub = childOfSub;
    }

    public InventorySaveData(InventorySystem invSystem)
    {
        this.invSystem = invSystem;
        position = Vector3.zero;
        rotation = Quaternion.identity;
        childOfSub = false;
    }
}

