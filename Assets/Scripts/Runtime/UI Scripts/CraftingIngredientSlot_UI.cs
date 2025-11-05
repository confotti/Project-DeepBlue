using UnityEngine;
using TMPro;

public class CraftingIngredientSlot_UI : ParentItemSlot_UI
{
    [SerializeField] private TextMeshProUGUI itemName;

    void Awake()
    {
        itemSprite.preserveAspect = true;
    }

    private void UpdateUISlot(CraftingIngredient ingredient)
    {
        itemSprite.sprite = ingredient.itemRequired.icon;
        itemCount.text = ingredient.amountRequired.ToString();
        itemName.text = ingredient.itemRequired.DisplayName;
    }
}
