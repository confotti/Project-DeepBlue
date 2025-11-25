using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftRecipe_UI : ParentItemSlot_UI
{
    [SerializeField] private TextMeshProUGUI craftedItemName;

    [SerializeField] private Button button;

    private CraftingDisplay parentDisplay;
    private CraftingRecipe recipe;

    void OnEnable()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnButtonClicked);
    }

    public void Init(CraftingRecipe recipe, CraftingDisplay parentDisplay)
    {
        this.recipe = recipe;
        this.parentDisplay = parentDisplay;
        UpdateUISlot(recipe);
    }

    public void OnButtonClicked()
    {
        if(parentDisplay == null) return;

        parentDisplay.UpdateChosenRecipe(recipe);
    }

    private void UpdateUISlot(CraftingRecipe recipe)
    {
        itemSprite.sprite = recipe.CraftedItem.Icon;
        itemCount.text = recipe.CraftedAmount.ToString();
        craftedItemName.text = recipe.CraftedItem.DisplayName;
    }
}
