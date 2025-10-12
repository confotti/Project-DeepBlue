using UnityEngine;

public class ItemPickUp : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;

    public void Interact(GameObject player)
    {
        Debug.Log($"Picked up {itemData.DisplayName}");
        
        var inventory = player.GetComponent<InventoryHolder>();

        if (!inventory) return;

        if (inventory.InventorySystem.AddToInventory(itemData, 1))
        {
            Destroy(this.gameObject);
        }
    }
}

