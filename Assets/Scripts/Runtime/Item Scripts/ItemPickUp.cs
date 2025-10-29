using UnityEngine;
using UnityEngine.Events;

public class ItemPickUp : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;
    [SerializeField] private int amountOfItem = 1;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteract interactor, out bool interactSuccessful)
    {
        var inventory = interactor.GetComponent<InventoryHolder>();

        if (!inventory)
        {
            interactSuccessful = false;
            return;
        }

        if (inventory.InventorySystem.AddToInventory(itemData, amountOfItem, out int remainingAmount))
        {
            if (remainingAmount == 0) Destroy(gameObject);
            else amountOfItem = remainingAmount;
        }

        interactSuccessful = true;
    }

    public void EndInteraction()
    {

    }
}

