using UnityEngine;

[System.Serializable]
public class EquipmentSlot : InventorySlot
{
    [SerializeField] private EquipmentType equipmentType;

    public EquipmentType EquipmentType => equipmentType;

    public EquipmentSlot(EquipmentType type) : base()
    {
        equipmentType = type;
    }

    //TODO: This should override the base AssignItem, but I don't know exactly how exactly
    public override void AssignItem(InventorySlot invSlot)
    {
        if (invSlot.ItemData.EquipmentType != equipmentType)
        {
            Debug.LogError($"Cannot assign {invSlot.ItemData.DisplayName} to {equipmentType} slot.");
            return;
        }

        base.AssignItem(new InventorySlot(invSlot.ItemData, 1));
    }
}

public enum EquipmentType
{
    None,
    Head,
    Feet, 
    OxygenTank
}
