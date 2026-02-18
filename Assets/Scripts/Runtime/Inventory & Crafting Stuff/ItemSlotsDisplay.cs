using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//Not really a reason to have this on every parent display. Could definitely remake it so there's one script that handles all of this instead of every parent having to handle their own slots. 

public class ItemSlotsDisplay : MonoBehaviour
{
    [SerializeField] protected MouseItemData mouseInventoryItem;


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
        if (clickedUISlot.AssignedInventorySlot.ItemData == null &&
            mouseInventoryItem.AssignedInventorySlot.ItemData != null &&
            clickedUISlot.AssignedInventorySlot.CanAssignItem(mouseInventoryItem.AssignedInventorySlot))
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
            else if (!isSameItem && clickedUISlot.AssignedInventorySlot.CanAssignItem(mouseInventoryItem.AssignedInventorySlot))
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
