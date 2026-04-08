using UnityEngine;
using TMPro;

public class CraftingIngredientSlot_UI : ParentItemSlot_UI
{
    [SerializeField] private TextMeshProUGUI itemName;

    void Awake()
    {
        //itemSprite.preserveAspect = true;
    }

    public void UpdateUISlot(ItemCost ingredient)
    {
        itemSprite.sprite = ingredient.ItemRequired.Icon;
        itemCount.text = ingredient.AmountRequired.ToString();
        itemName.text = ingredient.ItemRequired.DisplayName;
    }

}
