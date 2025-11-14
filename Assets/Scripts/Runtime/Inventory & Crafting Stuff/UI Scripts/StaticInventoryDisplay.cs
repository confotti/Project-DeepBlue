using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] slots;

    private void OnValidate()
    {
        Awake();
    }

    private void Awake()
    {
        slots = GetComponentsInChildren<InventorySlot_UI>();
    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += RefreshStaticDisplay;
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= RefreshStaticDisplay;
    }

    private void RefreshStaticDisplay()
    {
        if (inventoryHolder != null)
        {
            inventorySystem = inventoryHolder.InventorySystem;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
        }
        else Debug.LogWarning($"No inventory assigned to {this.gameObject}");

        AssignSlots(inventorySystem, 0);
    }

    protected override void Start()
    {
        base.Start();

        if(inventorySystem != null ) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
        RefreshStaticDisplay();
    }

    public override void AssignSlots(InventorySystem invToDisplay, int offset)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        for (int i = 0; i < slots.Length; i++)
        {
            slotDictionary.Add(slots[i], inventorySystem.InventorySlots[i]);
            slots[i].Init(inventorySystem.InventorySlots[i], this);
        }
    }

    private void OnDestroy()
    {
        inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }

}
