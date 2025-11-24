using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Unity.VisualScripting;

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
            var slot = new InventorySlot();
            slot.SlotChanged += InventorySlotChanged;
            inventorySlots.Add(slot);
        }
    }

    public void InventorySlotChanged(InventorySlot slot)
    {
        OnInventorySlotChanged(slot);
    }

    public void OnDestroy()
    {
        foreach (var slot in inventorySlots) slot.SlotChanged -= InventorySlotChanged;
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

    public bool RemoveFromInventory(InventoryItemData itemToRemove, int amountToRemove)
    {
        if(AmountOfItem(itemToRemove, out List<InventorySlot> invSlots) < amountToRemove)
        {
            Debug.Log($"Amount ({amountToRemove}) is more than we currently have ({AmountOfItem(itemToRemove)}). ");
            return false;
        }
        foreach (var slot in invSlots)
        {
            if(slot.StackSize > amountToRemove)
            {
                slot.RemoveFromStack(amountToRemove);
                OnInventorySlotChanged?.Invoke(slot);
                return true;
            }
            else
            {
                amountToRemove -= slot.StackSize;
                slot.ClearSlot();
                OnInventorySlotChanged?.Invoke(slot);
                if (amountToRemove == 0) return true;
            }
        }

        Debug.Log($"Something seems to have gone wrong I think? It should not be possible to get here");
        return false;
    }

    //Do any of our slots have the item to add in them? 
    //Outs a list of them and the bool is if any exists
    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlots)
    {
        invSlots = new List<InventorySlot>();
        foreach (var slot in InventorySlots)
        {
            if (slot.ItemData == itemToAdd)
                invSlots.Add(slot);
        }

        return invSlots.Count > 0;
    }

    public int AmountOfItem(InventoryItemData whatItem)
    {
        int amount = 0;
        foreach (var slot in InventorySlots)
        {
            if (slot.ItemData == whatItem)
                amount += slot.StackSize;
        }
        return amount;
    }

    public int AmountOfItem(InventoryItemData whatItem, out List<InventorySlot> invSlots)
    {
        int amount = 0;
        invSlots = new List<InventorySlot>();
        foreach (var slot in InventorySlots)
        {
            if (slot.ItemData != whatItem) continue;

            invSlots.Add(slot);
            amount += slot.StackSize;
        }
        return amount;
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

    public void ClearInventory()
    {
        foreach (var slot in inventorySlots)
        {
            slot.ClearSlot();
        }
    }
}
