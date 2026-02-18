using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        UpdateEquipmentStats();
    }

    public override void SlotClicked(InventorySlot_UI clickedUISlot)
    {
        base.SlotClicked(clickedUISlot);

        UpdateEquipmentStats();
    }

    private void UpdateEquipmentStats()
    {
        foreach(var slot in _equipmentSlotsUI)
        {
            if(slot._equipmentType == EquipmentType.OxygenTank)
            {
                if(slot.AssignedInventorySlot.ItemData == null) _playerStats.SetOxygenModifier(0);
                else _playerStats.SetOxygenModifier((int)slot.AssignedInventorySlot.ItemData.equipmentValue);
            }

            else if(slot._equipmentType == EquipmentType.Feet)
            {
                if(slot.AssignedInventorySlot.ItemData == null) GetComponent<PlayerMovement>().SwimmingState.SetSwimSpeedsModifiers(0);
                else GetComponent<PlayerMovement>().SwimmingState.SetSwimSpeedsModifiers(slot.AssignedInventorySlot.ItemData.equipmentValue, slot.AssignedInventorySlot.ItemData.equipmentValue + 1);
            }
        }
    }
}
