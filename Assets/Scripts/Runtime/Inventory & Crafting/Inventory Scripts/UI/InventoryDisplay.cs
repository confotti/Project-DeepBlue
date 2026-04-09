

public abstract class InventoryDisplay : ItemSlotsDisplay
{
    protected InventorySystem inventorySystem;
    public InventorySystem InventorySystem => inventorySystem;


    protected virtual void Start()
    {

    }

    //Implemented in child classes. 
    public abstract void AssignSlots(InventorySystem invToDisplay, int offset);
}
