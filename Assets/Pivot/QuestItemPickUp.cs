using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ActivationTarget
{
    public GameObject target;

    [Tooltip("Leave unchecked to keep the object active forever.")]
    public bool deactivateAfterTime = false;

    [Min(0)]
    public float activeTime = 5f;
}

public class QuestItemPickUp : MonoBehaviour, IInteractable
{
    public InventoryItemData itemData;
    [SerializeField] private int amountOfItem = 1;
    [SerializeField] private string interactText = "";

    [Header("Objects to Activate")]
    [SerializeField] private List<ActivationTarget> objectsToActivate = new ();

    [Header("Objects to Disable")]
    [SerializeField] private List<GameObject> objectsToDisable = new ();

    public string InteractText => interactText;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteract interactor)
    {
        var inventory = interactor.GetComponent<PlayerInventoryHolder>();
        if (!inventory) return;

        if (inventory.AddToInventory(itemData, amountOfItem, out int remainingAmount))
        {
            foreach (var activation in objectsToActivate)
            {
                if (activation.target == null)
                    continue;

                activation.target.SetActive(true);

                if (activation.deactivateAfterTime)
                {
                    AutoDisable autoDisable = activation.target.GetComponent<AutoDisable>();

                    if (autoDisable == null)
                        autoDisable = activation.target.AddComponent<AutoDisable>();

                    autoDisable.DisableAfter(activation.activeTime);
                }
            }

            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            if (remainingAmount == 0)
                Destroy(gameObject);
            else
                amountOfItem = remainingAmount;
        }
    }

    public void EndInteraction()
    {
    }
} 