using UnityEngine;
using TMPro;

public class CraftingIngredientSlot_UI : ParentItemSlot_UI
{
    [SerializeField] private TextMeshProUGUI itemName;

    void Awake()
    {
        //itemSprite.preserveAspect = true;
    }

    public void UpdateUISlot(CraftingIngredient ingredient)
    {
        itemSprite.sprite = ingredient.itemRequired.Icon;
        itemCount.text = ingredient.amountRequired.ToString();
        itemName.text = ingredient.itemRequired.DisplayName;
    }

}
