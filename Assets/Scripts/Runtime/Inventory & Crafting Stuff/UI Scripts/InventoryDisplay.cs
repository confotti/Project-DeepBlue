using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] MouseItemData mouseInventoryItem;

    protected InventorySystem inventorySystem;

    //Pair up UI slots with the system slots. 
    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary; 

    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    protected virtual void Start()
    {

    }

    //Implemented in child classes. 
    public abstract void AssignSlots(InventorySystem invToDisplay, int offset);

    protected virtual void UpdateSlot(InventorySlot updatedSlot)
    {
        foreach (var slot in slotDictionary)
        {
            if(slot.Value == updatedSlot) //Slot value - the "under the hood" inventory slot. 
            {
                slot.Key.UpdateUISlot(); //Slot key - the UI representation of the value. 
            }
        }
    }

    public void SlotClicked(InventorySlot_UI clickedUISlot)
    {
        // TODO: fix so this uses a more reasonable input thing, so instead of hard-coding it like this, make it based on the input action asset
        // Also, maybe swap it so it's a rightclick thing and not a shift thing, but IDK
        bool isShiftPressed = Keyboard.current.leftShiftKey.isPressed;

        // Clicked slot has an item and mouse doesn't have an item. 
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && 
            mouseInventoryItem.AssignedInventorySlot.ItemData == null)
        {
            //If player is holding shift key, try to split the stack and put half on mouse. 
            if (isShiftPressed && clickedUISlot.AssignedInventorySlot.SplitStack(out InventorySlot halfStackSlot)) //split stack
            {
                mouseInventoryItem.UpdateMouseSlot(halfStackSlot);
                clickedUISlot.UpdateUISlot();
                return;
            }
            else //Picks up if player is not trying to split or it is to few to split
            {
                mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);
                clickedUISlot.AssignedInventorySlot.ClearSlot();
                //clickedUISlot.ClearSlot();
                return;
            }
        }

        // Clicked slot doesnt have an item, but mouse does - place the mouse item there
        if(clickedUISlot.AssignedInventorySlot.ItemData == null &&
            mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
            clickedUISlot.UpdateUISlot();

            mouseInventoryItem.ClearSlot();
            return;
        }

        //Is the slot stack size + mouse stack size > items max stack size - Take from mouse
        //Both slots have an item - decide what to do
        if (clickedUISlot.AssignedInventorySlot.ItemData != null &&
            mouseInventoryItem.AssignedInventorySlot.ItemData != null)
        {
            bool isSameItem = clickedUISlot.AssignedInventorySlot.ItemData == mouseInventoryItem.AssignedInventorySlot.ItemData;

            //If they are the same - combine them
            if (isSameItem && clickedUISlot.AssignedInventorySlot.RoomLeftInStack(mouseInventoryItem.AssignedInventorySlot.StackSize))
            {
                clickedUISlot.AssignedInventorySlot.AddToStack(mouseInventoryItem.AssignedInventorySlot.StackSize);
                clickedUISlot.UpdateUISlot();
                mouseInventoryItem.ClearSlot();
                return;
            }
            else if (isSameItem && !clickedUISlot.AssignedInventorySlot.RoomLeftInStack(mouseInventoryItem.AssignedInventorySlot.StackSize, out int leftInStack))
            {
                if (leftInStack < 1) SwapSlots(clickedUISlot); //Stack is full, so swap them
                else //Slot is not at max, so take what's needed from the mouse inventory
                {
                    clickedUISlot.AssignedInventorySlot.AddToStack(leftInStack);
                    clickedUISlot.UpdateUISlot();

                    mouseInventoryItem.AssignedInventorySlot.RemoveFromStack(leftInStack);
                    mouseInventoryItem.UpdateMouseSlotUI();
                }
                return;
            }

            //If different - swap them
            else if (!isSameItem)
            {
                SwapSlots(clickedUISlot);
                return;
            }
        }
    }

    //Swaps what is on the mouse and what is on the clicked slot. 
    private void SwapSlots(InventorySlot_UI clickedUISlot)
    {
        var clonedSlot = new InventorySlot(mouseInventoryItem.AssignedInventorySlot.ItemData,
            mouseInventoryItem.AssignedInventorySlot.StackSize);
        mouseInventoryItem.ClearSlot();

        mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);

        //clickedUISlot.AssignedInventorySlot.ClearSlot();
        //clickedUISlot.ClearSlot();
        clickedUISlot.AssignedInventorySlot.AssignItem(clonedSlot);
        //clickedUISlot.UpdateUISlot();
    }
}
