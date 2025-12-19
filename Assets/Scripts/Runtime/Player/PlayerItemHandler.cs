using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    private ItemBehaviour _currentItem;

    private PlayerInputHandler _playerInputs;
    private PlayerInventoryHolder _playerInventory;
    public PlayerInventoryHolder PlayerInventory => _playerInventory;

    [SerializeField] private Transform _playerHead;
    public Transform PlayerHead => _playerHead;

    void Awake()
    {
        _playerInventory = GetComponent<PlayerInventoryHolder>();
    }

    private void OnEnable()
    {
        HotbarDisplay.EquipNewItem += EquipNewItem;
    }

    private void OnDisable()
    {
        HotbarDisplay.EquipNewItem -= EquipNewItem;
    }

    private void EquipNewItem(ItemBehaviour newItem)
    {
        if (_currentItem != null)
        {
            _currentItem.OnUnequip();
            _currentItem = null;
        } 

        if (newItem != null)
        {
            _currentItem = Instantiate(newItem, gameObject.transform);
            _currentItem.OnEquip(this);
        } 
    }
}
