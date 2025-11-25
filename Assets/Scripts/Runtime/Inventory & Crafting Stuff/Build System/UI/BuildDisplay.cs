using System;
using UnityEngine;
using UnityEngine.Events;

public class BuildDisplay : MonoBehaviour
{
    [SerializeField] private Building_UI _building_UI_Prefab;
    [SerializeField] private Transform _buildingListPanel;
    [SerializeField] private BuildingData[] _knownBuildings;

    public static UnityAction OnBuildDisplayRequested;
    public static UnityAction<BuildingData> OnPartChosen;

    public void DisplayBuildWindow()
    {
        RefreshListDisplay();
    }

    private void ClearSlots(Transform transformToDestroy)
    {
        for (int i = transformToDestroy.childCount - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(transformToDestroy.GetChild(i).gameObject, ObjectPoolManager.PoolType.UI);
        }
    }

    private void RefreshListDisplay()
    {
        ClearSlots(_buildingListPanel);

        foreach (var building in _knownBuildings)
        {
            var buildingSlot = ObjectPoolManager.SpawnObject(_building_UI_Prefab, _buildingListPanel, poolType: ObjectPoolManager.PoolType.UI);
            buildingSlot.Init(building);
        }
    }
}
