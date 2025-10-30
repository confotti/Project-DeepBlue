using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;

    protected override void Start()
    {
        base.Start();

    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay)
    {
        ClearSlots();
        inventorySystem = invToDisplay;
        AssignSlots(invToDisplay);
    }

    public override void AssignSlots(InventorySystem invToDisplay)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (invToDisplay == null) return;

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            //var uiSlot = Instantiate(slotPrefab, transform);
            var uiSlot = ObjectPoolManager.SpawnObject(slotPrefab, transform, poolType: ObjectPoolManager.PoolType.inventorySlots);

            slotDictionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i]);
        }
    }

    //TODO: Object pooling very important here
    private void ClearSlots()
    {
        /*
        foreach (var item in transform.Cast<Transform>())
        {
            //Destroy(item.gameObject);
            ObjectPoolManager.ReturnObjectToPool(item.gameObject, ObjectPoolManager.PoolType.inventorySlots);
        }
        */

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(transform.GetChild(i).gameObject, ObjectPoolManager.PoolType.inventorySlots);
        }

        if (slotDictionary != null) slotDictionary.Clear();
    }
}
