using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CraftingDisplay : MonoBehaviour
{
    [SerializeField] private CraftRecipe_UI craftRecipe_UI_Prefab;
    [SerializeField] private Transform recipeListPanel;

    [SerializeField] private CraftingIngredientSlot_UI ingredientSlotPrefab;
    [SerializeField] private Transform ingredientGrid;

    [Header("Item Display Section")]
    [SerializeField] private Image itemPreviewSprite;
    [SerializeField] private TextMeshProUGUI itemPreviewName;
    [SerializeField] private TextMeshProUGUI itemPreviewDescription;
    [SerializeField] private TextMeshProUGUI itemPreviewAmount;
    [SerializeField] private Button craftButton;

    private CraftingBench craftingBench;
    private CraftingRecipe chosenRecipe;

    public static UnityAction<CraftingBench> OnCraftingDisplayRequested;

    void Awake()
    {
        craftButton.onClick.AddListener(OnCraftButtonPressed);
    }

    void OnDestroy()
    {
        craftButton.onClick.RemoveListener(OnCraftButtonPressed);
    }

    public void DisplayCraftingWindow(CraftingBench craftBench)
    {
        ClearRecipePreview();

        craftingBench = craftBench;

        RefreshListDisplay();
    }

    private void RefreshListDisplay()
    {
        ClearSlots(recipeListPanel);

        foreach (var recipe in craftingBench.KnownRecipes)
        {
            var recipeSlot = ObjectPoolManager.SpawnObject(craftRecipe_UI_Prefab, recipeListPanel, poolType: ObjectPoolManager.PoolType.UI);
            recipeSlot.Init(recipe, this);
        }
    }

    private void ClearSlots(Transform transformToDestroy)
    {
        for (int i = transformToDestroy.childCount - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(transformToDestroy.GetChild(i).gameObject, ObjectPoolManager.PoolType.UI);
        }
    }

    private void DisplayItemPreview(CraftingRecipe recipe)
    {
        itemPreviewSprite.sprite = recipe.CraftedItem.Icon;
        itemPreviewSprite.color = Color.white;
        itemPreviewName.text = recipe.CraftedItem.DisplayName;
        itemPreviewDescription.text = recipe.CraftedItem.Description;
        //TODO: Make below only show it if it's bigger than 1
        itemPreviewAmount.text = recipe.CraftedAmount.ToString();
        craftButton.gameObject.SetActive(true);
    }

    private void ClearRecipePreview()
    {
        itemPreviewSprite.sprite = null;
        itemPreviewSprite.color = Color.clear;
        itemPreviewName.text = "";
        itemPreviewDescription.text = "";
        itemPreviewAmount.text = "";
        craftButton.gameObject.SetActive(false);

        ClearSlots(ingredientGrid);
    }

    public void UpdateChosenRecipe(CraftingRecipe recipe)
    {
        chosenRecipe = recipe;

        RefreshRecipeWindow();
    }

    private void RefreshRecipeWindow()
    {
        ClearSlots(ingredientGrid);

        foreach (var ingredient in chosenRecipe.Ingredients)
        {
            var ingredientSlot = ObjectPoolManager.SpawnObject(ingredientSlotPrefab, ingredientGrid, poolType: ObjectPoolManager.PoolType.UI);
            ingredientSlot.UpdateUISlot(ingredient);
        }

        DisplayItemPreview(chosenRecipe);
    }

    private void OnCraftButtonPressed()
    {
        if (craftingBench == null) return;

        craftingBench.TryCraft(chosenRecipe);

    }
    
}
