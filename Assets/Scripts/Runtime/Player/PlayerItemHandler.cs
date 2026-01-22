using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    private ItemBehaviour _currentItem;
    private InventoryItemData _currentItemData;
    private InventorySlot _currentSlot;

    private PlayerInputHandler _playerInputs;
    private PlayerInventoryHolder _playerInventory;
    public PlayerInventoryHolder PlayerInventory => _playerInventory;
    private PlayerInputHandler _inputHandler;

    [SerializeField] private Transform _playerHead;
    public Transform PlayerHead => _playerHead;

    void Awake()
    {
        _playerInventory = GetComponent<PlayerInventoryHolder>();
        _inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        HotbarDisplay.EquipNewSlot += EquipNewItem;

        _inputHandler.OnItemPrimary += OnItemPrimary;
        _inputHandler.OnItemSecondary += OnItemSecondary;
    }

    private void OnDisable()
    {
        HotbarDisplay.EquipNewSlot -= EquipNewItem;

        _inputHandler.OnItemPrimary -= OnItemPrimary;
        _inputHandler.OnItemSecondary -= OnItemSecondary;
    }

    private void OnItemPrimary()
    {
        //If cursor is not locked a UI is probably open. Either way the item in hand should not be used at this point. 
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if(_currentItem != null) _currentItem.PrimaryInput();
    }

    private void OnItemSecondary()
    {
        if(_currentItem != null) _currentItem.SecondaryInput();
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

        if (_currentItem != null && _currentItemData != _currentSlot.ItemData)
        {
            _currentItem.OnUnequip();
            _currentItem = null;
            _currentItemData = null;
        }
        else if (_currentItemData == _currentSlot.ItemData)
        {
            return;
        }


        if (_currentSlot.ItemData != null && _currentSlot.ItemData.itemPrefab != null)
        {
            _currentItem = Instantiate(_currentSlot.ItemData.itemPrefab, gameObject.transform);
            _currentItem.OnEquip(this);
            _currentItemData = _currentSlot.ItemData;
        }
    }

    public void ConsumeCurrentItem()
    {
        _currentSlot.RemoveFromStack(1);
    }
}
