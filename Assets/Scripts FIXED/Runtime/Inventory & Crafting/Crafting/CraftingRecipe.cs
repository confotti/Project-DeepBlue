using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Inventory System/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    //Currently does not support an output of multiple different items, fix if neccessary. 

    [SerializeField] private List<ItemCost> _ingredients;
    [SerializeField] private InventoryItemData _craftedItem;
    [SerializeField, Min(1)] private int _craftedAmount = 1;

    public List<ItemCost> Ingredients => _ingredients;
    public InventoryItemData CraftedItem => _craftedItem;
    public int CraftedAmount => _craftedAmount;

    void OnValidate()
    {
        for (int i = 0; i < _ingredients.Count; i++)
        {
            if (_ingredients[i].AmountRequired < 1) _ingredients[i] = new ItemCost(_ingredients[i].ItemRequired, 1);
        }
    }
}

[System.Serializable]
public struct ItemCost
{
    public InventoryItemData ItemRequired;

    public int AmountRequired;

    public ItemCost(InventoryItemData itemRequired, int amountRequired)
    {
        ItemRequired = itemRequired;
        AmountRequired = amountRequired;
    }
}