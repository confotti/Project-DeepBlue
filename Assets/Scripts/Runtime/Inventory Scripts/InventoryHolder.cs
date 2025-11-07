using SaveLoadSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public abstract class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int primaryInventorySize;
    [SerializeField] protected InventorySystem primaryInventorySystem;

    public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

    public static UnityAction<InventorySystem, int> OnDynamicInventoryDisplayRequested; //InvSystem to display, amount to offset display by

    //Updates the UI if we change anything in the holder. 
    private void OnValidate()
    {
        foreach (var slot in primaryInventorySystem.InventorySlots)
        {
            primaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);
        }
    }

    protected virtual void Awake()
    {
        primaryInventorySystem = new InventorySystem(primaryInventorySize);
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

