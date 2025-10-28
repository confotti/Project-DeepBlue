using UnityEngine;

/// <summary>
/// This is the items in the form of a scriptable object. 
/// It could be inherited from to have branched items, for example equipment, consumables, etc
/// </summary>

[CreateAssetMenu(menuName = "Inventory System/Inventory Item")]
public class InventoryItemData : ScriptableObject
{
    public string DisplayName;
    [TextArea(4, 4)] public string Description;

    public Sprite icon;
    public int MaxStackSize = 1;

    private void OnValidate()
    {
        if (MaxStackSize < 1)
        {
            MaxStackSize = 1;
            Debug.LogWarning("Max Stack Size cannot be less than 1");
        }
    }
}
