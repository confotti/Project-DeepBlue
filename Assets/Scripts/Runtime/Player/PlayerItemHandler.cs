using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    private ItemBehaviour _currentItem;
    private InventorySlot _currentSlot;

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
        HotbarDisplay.EquipNewSlot += EquipNewItem;
    }

    private void OnDisable()
    {
        HotbarDisplay.EquipNewSlot -= EquipNewItem;
    }

/*
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
*/

    private void EquipNewItem(InventorySlot slotToEquip)
    {
        _currentSlot = slotToEquip;

        if (_currentItem != null && _currentItem != _currentSlot.ItemData)
        {
            _currentItem.OnUnequip();
            _currentItem = null;
        }
        else if (_currentItem == _currentSlot.ItemData) return;

        if (_currentSlot.ItemData != null)
        {
            _currentItem = Instantiate(_currentSlot.ItemData.itemPrefab, gameObject.transform);
            _currentItem.OnEquip(this);
        }
    }

    public void ConsumeCurrentItem()
    {
        _currentSlot.RemoveFromStack(1);
    }
}
