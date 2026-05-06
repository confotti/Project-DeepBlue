using UnityEngine;
using TMPro;

public class CraftingIngredientSlot_UI : ParentItemSlot_UI
{
    [SerializeField] private TextMeshProUGUI itemName;

    public void UpdateUISlot(ItemCost ingredient, int amountInInventory)
    {
        itemSprite.sprite = ingredient.ItemRequired.Icon;
        itemCount.text = amountInInventory.ToString() + "/" + ingredient.AmountRequired.ToString();
        itemName.text = ingredient.ItemRequired.DisplayName;

        //If too expensive
        if (amountInInventory >= ingredient.AmountRequired)
        {
            itemSprite.color = Color.white;
            itemCount.color = Color.white;
            itemName.color = Color.white;
        }
        else
        {
            itemSprite.color = Color.red;
            itemCount.color = Color.red;
            itemName.color = Color.red;
        }
    }

}
