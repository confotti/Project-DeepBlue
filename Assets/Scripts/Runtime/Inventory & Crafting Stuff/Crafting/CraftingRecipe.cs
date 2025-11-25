using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    //Currently does not support an output of multiple different items, fix if neccessary. 

    [SerializeField] private List<ItemCost> ingredients;
    [SerializeField] private InventoryItemData craftedItem;
    [SerializeField, Min(1)] private int craftedAmount = 1;

    public List<ItemCost> Ingredients => ingredients;
    public InventoryItemData CraftedItem => craftedItem;
    public int CraftedAmount => craftedAmount;
}

[System.Serializable]
public struct ItemCost
{
    public InventoryItemData itemRequired;
    public int amountRequired;

    public ItemCost(InventoryItemData itemRequired, int amountRequired)
    {
        this.itemRequired = itemRequired;
        this.amountRequired = amountRequired;
    }
}