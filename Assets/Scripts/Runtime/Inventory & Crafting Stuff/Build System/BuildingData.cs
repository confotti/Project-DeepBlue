using UnityEngine;

[CreateAssetMenu(menuName = "Building System/Building Data")]
public class BuildingData : ScriptableObject
{
    public string DisplayName;
    public Sprite Icon;
    public Building Prefab;
    public BuildingCategory Category;
    public ItemCost[] Cost;
}

public enum BuildingCategory
{
    Crafting,
    Storage
}

