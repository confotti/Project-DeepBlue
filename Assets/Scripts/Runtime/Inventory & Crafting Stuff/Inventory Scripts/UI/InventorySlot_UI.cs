using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot_UI : ParentItemSlot_UI
{
    [SerializeField] private GameObject _slotHighlight;

    [SerializeField] private InventorySlot assignedInventorySlot;
    [SerializeField] private EquipmentType _equipmentType = EquipmentType.None;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;

    private Button button;

    
    
    public ItemSlotsDisplay ParentDisplay { get; private set; }

    private void OnValidate()
    {
        UpdateUISlot();
    }

    private void Awake()
    {
        ClearSlot();
        //itemSprite.preserveAspect = true;

        button = GetComponent<Button>();
        button?.onClick.AddListener(OnUISlotClick);

    }

    private void OnDestroy()
    {
        button?.onClick.RemoveListener(OnUISlotClick);
    }

    void OnDisable()
    {
        if (assignedInventorySlot != null)
        {
            assignedInventorySlot.SlotChanged -= UpdateUISlot;
            assignedInventorySlot = null;
        }
    }

    public void Init(InventorySlot slot, ItemSlotsDisplay parentDisplay)
    {
        if(assignedInventorySlot != null) assignedInventorySlot.SlotChanged -= UpdateUISlot;
        ParentDisplay = parentDisplay;
        assignedInventorySlot = slot;
        assignedInventorySlot.SlotChanged += UpdateUISlot;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot)
    {
        if(slot.ItemData != null)
        {
            itemSprite.sprite = slot.ItemData.Icon;
            itemSprite.color = Color.white;

            if (slot.StackSize > 1) itemCount.text = slot.StackSize.ToString();
            else itemCount.text = "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void UpdateUISlot()
    {
        if(assignedInventorySlot != null) UpdateUISlot(assignedInventorySlot);
        else ClearSlot();
    }

    //Clears out the slot and updates UI. 
    public void ClearSlot()
    {
        //assignedInventorySlot.ClearSlot();
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void OnUISlotClick()
    {
        ParentDisplay?.SlotClicked(this);
    }

    internal void ToggleHighlight()
    {
        _slotHighlight.SetActive(!_slotHighlight.activeSelf);
    }
}
