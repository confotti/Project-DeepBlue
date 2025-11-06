using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder
{
    [SerializeField] private int secondaryInventorySize;
    [SerializeField] protected InventorySystem secondaryInventorySystem;

    public InventorySystem SecondaryInventorySystem => secondaryInventorySystem;

    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested;

    protected override void Awake()
    {
        base.Awake();

        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            OnPlayerBackpackDisplayRequested?.Invoke(secondaryInventorySystem);
        }
    }

    //TODO: This will always try to add to the hotbar before looking if item exists in the backpack. 
    //Instead I want it to first look if there already is a non-full stack anywhere in the two systems, add to that,
    //and then if there's still more to be added do this. 
    public bool AddToInventory(InventoryItemData data, int amount, out int amountRemaining, bool spawnItemOnFail = false)
    {

        //Look for non-full stacks first. Probably need to make an AddToInventoryOnlyFillAlreadyAssigned function
        //in the InventorySystem class or something like that. 

        if (primaryInventorySystem.AddToInventory(data, amount, out int remainingAmount))
        {
            amountRemaining = 0;
            return true;
        }
        else if (secondaryInventorySystem.AddToInventory(data, remainingAmount, out remainingAmount))
        {
            amountRemaining = 0;
            return true;
        }


        if (spawnItemOnFail)
        {
            //TODO: Drop from the player the remainingAmount here probably, 
            //but depends on how we want to handle trying to pick-up items with full inventory. 
        }

        amountRemaining = remainingAmount;
        return false;
    }

    public Dictionary<InventoryItemData, int> GetAllItemsHeld()
    {
        var d1 = primaryInventorySystem.GetAllItemsHeld();

        foreach (var item in secondaryInventorySystem.GetAllItemsHeld())
        {
            if (!d1.ContainsKey(item.Key))
            {
                d1.Add(item.Key, item.Value);
            }
            else d1[item.Key] += item.Value;
        }

        return d1;
    }

    public void RemoveItemFromInventory(InventoryItemData itemData, int amount)
    {
        //TODO: Have to create RemoveItemFromInventory() function in InventorySystem. 
    }
        
}
