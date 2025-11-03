using UnityEngine;
using UnityEngine.Events;

public class CraftingBench : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingRecipe activeRecipe;

    private PlayerInventoryHolder playerInventory;

    public UnityAction<IInteractable> OnInteractionComplete { get ; set ; }

    public void Interact(PlayerInteract interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;

        playerInventory = interactor.GetComponent<PlayerInventoryHolder>();

        if (playerInventory == null) return;

        if (CheckIfCanCraft())
        {
            //Stannat vid 55:00, fortsätt senare. 
        }

        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        
    }

    private bool CheckIfCanCraft()
    {
        var itemsHeld = playerInventory.PrimaryInventorySystem.GetAllItemsHeld();

        foreach (var ingredient in activeRecipe.Ingredients)
        {
            if (!itemsHeld.TryGetValue(ingredient.itemRequired, out int amountHeld)) return false;

            if(ingredient.amountRequired < amountHeld)
            {
                return false;
            }
        }

        return true;
    }


}
