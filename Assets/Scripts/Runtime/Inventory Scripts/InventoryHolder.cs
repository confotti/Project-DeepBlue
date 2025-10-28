using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    [SerializeField] protected InventorySystem inventorySystem;

    public InventorySystem InventorySystem => inventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    private void OnValidate()
    {
        foreach (var slot in inventorySystem.InventorySlots)
        {
            inventorySystem.OnInventorySlotChanged?.Invoke(slot);
        }
    }

    private void Awake()
    {
        inventorySystem = new InventorySystem(inventorySize);
    }
}
