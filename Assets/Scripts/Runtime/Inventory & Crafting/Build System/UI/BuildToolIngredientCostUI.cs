using System;
using UnityEngine;

public class BuildToolIngredientCostUI : MonoBehaviour
{
    public static Action<BuildingData> OnUpdateUI;

    [SerializeField] private CraftingIngredientSlot_UI _craftingIngredientSlot_UI;



    private void Awake()
    {
        OnUpdateUI += UpdateUI;


    }

    private void OnDestroy()
    {
        OnUpdateUI -= UpdateUI;
    }

    private void UpdateUI(BuildingData data)
    {
        if(data == null)
        {
            ClearSlots(transform);
        }
        else
        {
            foreach (var cost in data.Cost)
            {
                //TODO: Need to show if the player has the items or not in some way
                var ingredientSlot = ObjectPoolManager.SpawnObject(_craftingIngredientSlot_UI, transform, poolType: ObjectPoolManager.PoolType.UI);
                ingredientSlot.UpdateUISlot(cost);
            }
        }
    }

    private void ClearSlots(Transform transformToDestroy)
    {
        for (int i = transformToDestroy.childCount - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(transformToDestroy.GetChild(i).gameObject, ObjectPoolManager.PoolType.UI);
        }
    }
}
