using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int primaryInventorySize;
    [SerializeField] protected InventorySystem primaryInventorySystem;

    public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;

    //Updates the UI if we change anything in the holder. 
    private void OnValidate()
    {
        foreach (var slot in primaryInventorySystem.InventorySlots)
        {
            primaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);
        }
    }

    protected virtual void Awake()
    {
        primaryInventorySystem = new InventorySystem(primaryInventorySize);
    }
}
