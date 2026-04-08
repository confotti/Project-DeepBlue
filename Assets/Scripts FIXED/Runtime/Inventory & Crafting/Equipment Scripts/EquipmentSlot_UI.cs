using UnityEngine;

public class EquipmentSlot_UI : InventorySlot_UI
{
    [SerializeField] public EquipmentType _equipmentType = EquipmentType.None;

    public override void OnDisable()
    {
        
    }

    public override void ClearSlot()
    {
        //Set to like a base image for the equipment instead
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;

        itemCount.text = "";
    }
}
