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

    void OnDisable()
    {
        ClearSlots();
    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay, int offset)
    {
        ClearSlots();
        inventorySystem = invToDisplay;
        AssignSlots(invToDisplay, offset);
    }

    public override void AssignSlots(InventorySystem invToDisplay, int offset)
    {
        if (invToDisplay == null) return;

        for (int i = offset; i < invToDisplay.InventorySize; i++)
        {
            //var uiSlot = Instantiate(slotPrefab, transform);
            var uiSlot = ObjectPoolManager.SpawnObject(slotPrefab, gridParent, poolType: ObjectPoolManager.PoolType.UI);

            uiSlot.Init(invToDisplay.InventorySlots[i], this);
        }
    }

    private void ClearSlots()
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(gridParent.GetChild(i).gameObject, ObjectPoolManager.PoolType.UI);
        }
    }
    
}
