using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class InventorySystem
{
    [SerializeField] private List<InventorySlot> inventorySlots;

    public List<InventorySlot> InventorySlots => inventorySlots;
    public int InventorySize => inventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size) //Constructor that sets the amount of slots. 
    {
        inventorySlots = new List<InventorySlot>();

        for (int i = 0; i < size; i++)
        {
            inventorySlots.Add(new InventorySlot());
        }
    }

    //Dont think this splits correctly if it amountToAdd doesnt fit in a slot
    public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd, out int remainingAmount)
    {
        //Check whether item exists in inventory
        if (ContainsItem(itemToAdd, out List<InventorySlot> invSlots)) 
        {
            foreach(var slot in invSlots)
            {
                if(slot.RoomLeftInStack(amountToAdd))
                {
                    slot.AddToStack(amountToAdd);
                    OnInventorySlotChanged?.Invoke(slot);
                    remainingAmount = 0;
                    return true;
                }
                else
                {
                    slot.RoomLeftInStack(amountToAdd, out int extraSpaceInStack);
                    slot.AddToStack(extraSpaceInStack);
                    OnInventorySlotChanged?.Invoke(slot);
                    amountToAdd -= extraSpaceInStack;
                }
            }
        }

        //Gets the first available slot
        if (HasFreeSlot(out InventorySlot freeSlot)) 
        {
            freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
            OnInventorySlotChanged?.Invoke(freeSlot);
            remainingAmount = 0;
            return true;

            //TODO: fix this
            //Currently doesnt check for max stackSize and just fills the first free slot with
            //whatever is left to add. Fix later
            //Allegedly he fixes later, otherwise I will do it myself
        }

        remainingAmount = amountToAdd;
        return false;
    }

    //Do any of our slots have the item to add in them? 
    //Outs a list of them and the bool is if any exists
    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlots)
    {
        invSlots = InventorySlots.Where(slot => slot.ItemData == itemToAdd).ToList();
        return invSlots.Count > 0;
    }

    //Do we have a free slot? Returns true if we do and outs the first slot. 
    public bool HasFreeSlot(out InventorySlot freeSlot)
    {
        freeSlot = InventorySlots.FirstOrDefault(slot => slot.ItemData == null);
        return freeSlot != null;
    }

    /// <summary>
    /// Returns a dictionary of each item the inventory contains, and the count, ignoring stack size
    /// </summary>
    /// <returns>distinctItem</returns>
    public Dictionary<InventoryItemData, int> GetAllItemsHeld()
    {
        var distinctItems = new Dictionary<InventoryItemData, int>();

        foreach (var item in InventorySlots)
        {
            if (item.ItemData == null) continue;

            if (!distinctItems.ContainsKey(item.ItemData))
            {
                distinctItems.Add(item.ItemData, item.StackSize);
            }
            else distinctItems[item.ItemData] += item.StackSize;
        }

        return distinctItems;
    }
}
