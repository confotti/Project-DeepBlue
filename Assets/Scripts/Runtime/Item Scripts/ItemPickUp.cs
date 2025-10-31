using UnityEngine;
using UnityEngine.Events;

public class ItemPickUp : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;
    [SerializeField] private int amountOfItem = 1;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteract interactor, out bool interactSuccessful)
    {
        var inventory = interactor.GetComponent<PlayerInventoryHolder>();

        if (!inventory)
        {
            interactSuccessful = false;
            return;
        }

        if (inventory.AddToInventory(itemData, amountOfItem, out int remainingAmount))
        {
            //TODO: Should probably pool this, but not sure at this moment. 
            if (remainingAmount == 0) Destroy(gameObject);
            else amountOfItem = remainingAmount;
        }

        interactSuccessful = true;
    }

    public void EndInteraction()
    {

    }
}

