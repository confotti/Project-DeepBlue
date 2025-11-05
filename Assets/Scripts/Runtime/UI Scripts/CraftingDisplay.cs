using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingDisplay : MonoBehaviour
{
    [SerializeField] private CraftingIngredientSlot_UI ingredientSlotPrefab;
    [SerializeField] private Transform ingredientGrid;

    [Header("Item Display Section")]
    [SerializeField] private Image itemPreviewSprite;
    [SerializeField] private TextMeshProUGUI itemPreviewName;
    [SerializeField] private TextMeshProUGUI itemPreviewDescription;

    private CraftingBench craftingBench;

    // Fortsätt från 1:29:25 https://www.youtube.com/watch?v=kSCY3b9kKsU
}
