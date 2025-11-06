using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingBench : MonoBehaviour, IInteractable
{
    [SerializeField] private List<CraftingRecipe> knownRecipes;

    private PlayerInventoryHolder playerInventory;

    public UnityAction<IInteractable> OnInteractionComplete { get ; set ; }

    public List<CraftingRecipe> KnownRecipes => knownRecipes;

    public void Interact(PlayerInteract interactor, out bool interactSuccessful)
    {
        playerInventory = interactor.GetComponent<PlayerInventoryHolder>();

        if (playerInventory != null)
        {
            CraftingDisplay.OnCraftingDisplayRequested?.Invoke(this);
            /*
            if (CheckIfCanCraft())
            {
                foreach (var ingredient in activeRecipe.Ingredients)
                {
                    //Have to create RemoveItemFromInventory() function in InventorySystem. Stopped video at 56:35. 
                    //playerInventory.PrimaryInventorySystem.RemoveItemFromInventory(ingredient.itemRequired, ingredient.amountRequired);
                }

                playerInventory.AddToInventory(activeRecipe.CraftedItem, activeRecipe.CraftedAmount, out int remainingAmount, true);
            }

            EndInteraction();
            interactSuccessful = true;
        */}
        else
        {
            interactSuccessful = false;
        }
        //Remove below
        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        
    }

    //This only checks in one of the 2 PlayerInventorys InventorySystems.
    //TODO: Do something about that. 
    private bool CheckIfCanCraft()
    {
        //TODO: Give playerInventory a GetAllItemsHeld() function so it can look through both inventories. 
        var itemsHeld = playerInventory.PrimaryInventorySystem.GetAllItemsHeld();
        /*
        foreach (var ingredient in activeRecipe.Ingredients)
        {
            if (!itemsHeld.TryGetValue(ingredient.itemRequired, out int amountHeld)) return false;

            if (ingredient.amountRequired > amountHeld)
            {
                return false;
            }
        }
        */
        return true;
    }


}
