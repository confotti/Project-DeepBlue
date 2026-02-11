using System;
using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] protected InventorySlot_UI[] slots;

    private void OnValidate()
    {
        slots = GetComponentsInChildren<InventorySlot_UI>();
    }

    private void Awake()
    {
        slots = GetComponentsInChildren<InventorySlot_UI>();
    }

    protected virtual void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += RefreshStaticDisplay;
    }

    protected virtual void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= RefreshStaticDisplay;
    }

    private void RefreshStaticDisplay()
    {
        if (inventoryHolder != null)
        {
            inventorySystem = inventoryHolder.InventorySystem;
        }
        else Debug.LogWarning($"No inventory assigned to {this.gameObject}");

        AssignSlots(inventorySystem, 0);
    }

    protected override void Start()
    {
        base.Start();

        RefreshStaticDisplay();
    }

    public override void AssignSlots(InventorySystem invToDisplay, int offset)
    {

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Init(inventorySystem.InventorySlots[i], this);
        }
    }

}
