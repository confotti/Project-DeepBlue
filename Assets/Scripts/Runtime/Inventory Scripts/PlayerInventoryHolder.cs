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
    public bool AddToInventory(InventoryItemData data, int amount, out int amountRemaining)
    {

        //Look for non-full stacks first. Probably need to make an AddToInventoryOnlyFillAlreadyAssigned function
            //in the InventorySystem class or something like that. 

        if(primaryInventorySystem.AddToInventory(data, amount, out int remainingAmount))
        {
            amountRemaining = 0;
            return true;
        }
        else if(secondaryInventorySystem.AddToInventory(data, remainingAmount, out remainingAmount))
        {
            amountRemaining = 0;
            return true;
        }

        amountRemaining = remainingAmount;
        return false;
    }
}
