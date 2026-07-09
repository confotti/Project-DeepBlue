using UnityEngine;
using UnityEngine.Events;

public class QuestItemPickUp : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;
    [SerializeField] private int amountOfItem = 1;
    [SerializeField] private string interactText = "";
    [SerializeField] private GameObject _objectToActivate;

    public string InteractText => interactText;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteract interactor)
    {
        var inventory = interactor.GetComponent<PlayerInventoryHolder>();
        if (!inventory) return;

        if (inventory.AddToInventory(itemData, amountOfItem, out int remainingAmount))
        {
            if (_objectToActivate != null)
                _objectToActivate.SetActive(true);

            if (remainingAmount == 0) Destroy(gameObject);
            else amountOfItem = remainingAmount;
        }
    }

    public void EndInteraction()
    {
    }
} 