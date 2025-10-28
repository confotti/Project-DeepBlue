using UnityEngine;

public class ItemPickUp : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;
    [SerializeField] private int amountOfItem = 1;

    public void Interact(GameObject player)
    {
        var inventory = player.GetComponent<InventoryHolder>();

        if (!inventory) return;

        if (inventory.InventorySystem.AddToInventory(itemData, amountOfItem, out int remainingAmount))
        {
            if(remainingAmount == 0) Destroy(gameObject);
            else amountOfItem = remainingAmount;
        }
    }
}

