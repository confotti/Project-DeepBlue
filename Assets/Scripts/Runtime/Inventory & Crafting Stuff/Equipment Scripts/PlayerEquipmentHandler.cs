using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipmentHandler : ItemSlotsDisplay
{

    private EquipmentSlot_UI[] _equipmentSlotsUI;

    [Header("References")]
    [SerializeField] private GameObject _playerEquipmentDisplay;
    private PlayerStats _playerStats;

    void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
        if(_playerEquipmentDisplay != null) _equipmentSlotsUI = _playerEquipmentDisplay.GetComponentsInChildren<EquipmentSlot_UI>();

        foreach(var slot in _equipmentSlotsUI)
        {
            //Have to load in once a save system is created. 
            slot.Init(new EquipmentSlot(slot._equipmentType), this);
        }
    }

    void Start()
    {
        EquipAllEquipmentEffects();
    }

    public override void SlotClicked(InventorySlot_UI clickedUISlot)
    {
        
        // Clicked slot has an item and mouse doesn't have an item. 
        if (clickedUISlot.AssignedInventorySlot.ItemData != null &&
            mouseInventoryItem.AssignedInventorySlot.ItemData == null)
        {
            //Picks up if player is not trying to split or it is to few to split
            mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);

            //This is new
            clickedUISlot.AssignedInventorySlot.ItemData.RemoveAllEquipmentEffects(_playerStats);

            clickedUISlot.AssignedInventorySlot.ClearSlot();
            return;

        }

        // Clicked slot doesnt have an item, but mouse does - place the mouse item there
        if (clickedUISlot.AssignedInventorySlot.ItemData == null &&
            mouseInventoryItem.AssignedInventorySlot.ItemData != null &&
            clickedUISlot.AssignedInventorySlot.CanAssignItem(mouseInventoryItem.AssignedInventorySlot))
        {
            clickedUISlot.AssignedInventorySlot.AssignItem(mouseInventoryItem.AssignedInventorySlot);
            clickedUISlot.UpdateUISlot();

            //This is new in this base class
            clickedUISlot.AssignedInventorySlot.ItemData.ApplyAllEquipmentEffects(_playerStats);

            mouseInventoryItem.ClearSlot();
            return;
        }

        //Both slots have an item - decide what to do
        //This cannot have multiple of the same item on an equipmentslot right now, but that is probably the way it's gonna be either way. 
        if (clickedUISlot.AssignedInventorySlot.ItemData != null && mouseInventoryItem.AssignedInventorySlot.ItemData != null 
            && clickedUISlot.AssignedInventorySlot.CanAssignItem(mouseInventoryItem.AssignedInventorySlot))
        {
            SwapSlots(clickedUISlot);
            return;
        }
    }

    protected override void SwapSlots(InventorySlot_UI clickedUISlot)
    {
        var clonedSlot = new InventorySlot(mouseInventoryItem.AssignedInventorySlot.ItemData,
            mouseInventoryItem.AssignedInventorySlot.StackSize);
        mouseInventoryItem.ClearSlot();

        mouseInventoryItem.UpdateMouseSlot(clickedUISlot.AssignedInventorySlot);

        //If you swap from one oxygen tank to another it will clamp you between 0 and baseOxygen, which might not be the best. 
        clickedUISlot.AssignedInventorySlot.ItemData.RemoveAllEquipmentEffects(_playerStats);
        clickedUISlot.AssignedInventorySlot.AssignItem(clonedSlot);
        clickedUISlot.AssignedInventorySlot.ItemData.ApplyAllEquipmentEffects(_playerStats);
    }

    private void EquipAllEquipmentEffects()
    {
        foreach(var slot in _equipmentSlotsUI)
        {
            if(slot == null) continue;
            slot.AssignedInventorySlot.ItemData.ApplyAllEquipmentEffects(_playerStats);
        }
    }
}
