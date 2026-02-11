using UnityEngine;
using UnityEngine.Events;

public class CrackRepair : MonoBehaviour, IInteractable
{
    [Header("Required Item")]
    public InventoryItemData repairTorchItemData;
    private bool repaired = false;
    public string InteractText => repaired ? "Already Repaired" : "Repair Crack";
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    
    public void EndInteraction() { }
    
    public void Interact(PlayerInteract interactor)
    {
        if (repaired)
            return;

        PlayerInventoryHolder inventoryHolder = interactor.GetComponent<PlayerInventoryHolder>();
        if (inventoryHolder == null)
            return;

        if (inventoryHolder.InventorySystem.ContainsItem(repairTorchItemData, out var itemAmount))
        {
            Repair();
        }
        else
        {
            Debug.Log("Repair Torch required!");
        }
    }

    private void Repair()
    {
        repaired = true;

        gameObject.SetActive(false);

        OnInteractionComplete?.Invoke(this);
    }
} 