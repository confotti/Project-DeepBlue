using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;
    [SerializeField] private Transform gridParent;

    protected override void Start()
    {
        base.Start();

    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay, int offset)
    {
        ClearSlots();
        inventorySystem = invToDisplay;
        if(inventorySystem != null) inventorySystem.OnInventorySlotChanged += UpdateSlot;
        AssignSlots(invToDisplay, offset);
    }

    public override void AssignSlots(InventorySystem invToDisplay, int offset)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (invToDisplay == null) return;

        for (int i = offset; i < invToDisplay.InventorySize; i++)
        {
            //var uiSlot = Instantiate(slotPrefab, transform);
            var uiSlot = ObjectPoolManager.SpawnObject(slotPrefab, gridParent, poolType: ObjectPoolManager.PoolType.UI);

            slotDictionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i], this);
        }
    }

    private void ClearSlots()
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(gridParent.GetChild(i).gameObject, ObjectPoolManager.PoolType.UI);
        }

        if (slotDictionary != null) slotDictionary.Clear();
    }

    private void OnDisable()
    {
        if (inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }
}
