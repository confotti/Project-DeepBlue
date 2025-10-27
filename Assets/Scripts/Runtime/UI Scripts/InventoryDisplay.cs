using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public abstract class InventoryDisplay : MonoBehaviour
{
    [SerializeField] MouseItemData mouseInventoryItem;
    protected InventorySystem inventorySystem;
    protected Dictionary<InventorySlot_UI, InventorySlot> slotDictionary;

    public InventorySystem InventorySystem => inventorySystem;
    public Dictionary<InventorySlot_UI, InventorySlot> SlotDictionary => slotDictionary;

    protected virtual void Start()
    {

    }

    public abstract void AssignSlot(InventorySystem invToDisplay);

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

    public void SlotClicked(InventorySlot_UI clickedSlot)
    {
        Debug.Log("Slot clicked");
    }
}
