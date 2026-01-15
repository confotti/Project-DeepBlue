using System;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    [NonSerialized] private InventoryItemData itemData; //Reference to the data
    [SerializeField, InspectorReadOnly] private int itemID = -1;
    [SerializeField] private int stackSize; //Current stack size - how many we have in this slot

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;

    public Action<InventorySlot> SlotChanged;

    //Constructor to make a occupied slot. 
    public InventorySlot(InventoryItemData source, int amount) 
    {
        itemData = source;
        itemID = itemData.ID;
        stackSize = amount;
    }

    //Constructor for an empty slot. 
    public InventorySlot() 
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        itemData = null;
        stackSize = -1;
        itemID = -1;
        SlotChanged?.Invoke(this);
    }

    //Assigns an item to the slot
    public void AssignItem(InventorySlot invSlot) 
    {
        itemData = invSlot.ItemData;
        itemID = itemData.ID;
        stackSize = invSlot.stackSize;
        SlotChanged?.Invoke(this);
    }

    //Update slot directly, this or AssignItem may be redundant
    public void UpdateInventorySlot(InventoryItemData data, int amount) 
    {
        itemData = data;
        itemID = itemData.ID;
        stackSize = amount;
        SlotChanged?.Invoke(this);
    }

    //Is there enough room in the stack for the amount we want to add, this outputs how much more fits
    public bool RoomLeftInStack(int amountToAdd, out int remainingSpaceInStack)
    {
        remainingSpaceInStack = ItemData.MaxStackSize - stackSize;

        return RoomLeftInStack(amountToAdd);
    }

    //Is there enough room in the stack for the amount we want to add
    public bool RoomLeftInStack(int amountToAdd)
    {
        return stackSize + amountToAdd <= itemData.MaxStackSize;
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
        SlotChanged?.Invoke(this);
    }

    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
        if (stackSize <= 0) ClearSlot();
        else SlotChanged?.Invoke(this);
    }

    public bool SplitStack(out InventorySlot splitStack)
    {
        if(stackSize <= 1) //Is there enough to split?
        {
            splitStack = null;
            return false;
        }
        
        int halfStack = Mathf.RoundToInt(stackSize / 2); //Get half the stack. 
        RemoveFromStack(halfStack);

        SlotChanged?.Invoke(this);

        //Creates a copy with half the stack size. 
        splitStack = new InventorySlot(itemData, halfStack); 
        return true;
    }
}
