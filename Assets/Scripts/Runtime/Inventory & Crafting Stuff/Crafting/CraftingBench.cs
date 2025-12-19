using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CraftingBench : MonoBehaviour, IInteractable
{
    //TODO: Make a CraftingStationParent class or similar. 
    [SerializeField] private string interactText = "";
    public string InteractText => interactText;

    [SerializeField] private List<CraftingRecipe> knownRecipes;

    private PlayerInventoryHolder playerInventory;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<CraftingRecipe> KnownRecipes => knownRecipes;

    public void Interact(PlayerInteract interactor)
    {
        playerInventory = interactor.GetComponent<PlayerInventoryHolder>();

        if (playerInventory != null)
        {
            CraftingDisplay.OnCraftingDisplayRequested?.Invoke(this);

            EndInteraction();

        }
    }

    public void EndInteraction()
    {

    }

    private bool CheckIfCanCraft(CraftingRecipe recipe)
    {
        var itemsHeld = playerInventory.InventorySystem.GetAllItemsHeld();

        foreach (var ingredient in recipe.Ingredients)
        {
            if (!itemsHeld.TryGetValue(ingredient.ItemRequired, out int amountHeld)) return false;

            if (ingredient.AmountRequired > amountHeld)
            {
                return false;
            }
        }

        return true;
    }

    public bool TryCraft(CraftingRecipe recipe)
    {
        if (CheckIfCanCraft(recipe))
        {
            foreach (var ingredient in recipe.Ingredients)
            {
                playerInventory.RemoveItemFromInventory(ingredient.ItemRequired, ingredient.AmountRequired);
            }

            playerInventory.AddToInventory(recipe.CraftedItem, recipe.CraftedAmount, out int remainingAmount, true);

            return true;
        }
        else return false;
    }


}
