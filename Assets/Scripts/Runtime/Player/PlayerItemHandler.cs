using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerItemHandler : MonoBehaviour
{
    private ItemBehaviour _currentItem;
    private InventoryItemData _currentItemData;
    private InventorySlot _currentSlot;

    private PlayerInputHandler _playerInputs;
    private PlayerInventoryHolder _playerInventory;
    public PlayerInventoryHolder PlayerInventory => _playerInventory;
    private PlayerInputHandler _inputHandler;
    public PlayerInputHandler InputHandler => _inputHandler;

    [SerializeField] private Transform _playerHead;
    public Transform PlayerHead => _playerHead;

    [SerializeField] private Transform placeToolPoint;

    [SerializeField] private ChainIKConstraint _holdingObjectConstraint;

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
            _holdingObjectConstraint.weight = 0.0f;
        }
        else if (_currentItemData == _currentSlot.ItemData)
        {
            return;
        }


        if (_currentSlot.ItemData != null && _currentSlot.ItemData.itemPrefab != null)
        {
            _currentItem = Instantiate(_currentSlot.ItemData.itemPrefab, gameObject.transform);
            _holdingObjectConstraint.weight = 0.6f;

            //_currentItem = Instantiate(_currentSlot.ItemData.itemPrefab, placeToolPoint);

            //A little bit cursed, but the scales make this the best option
            _currentItem.transform.rotation = placeToolPoint.transform.rotation * Quaternion.Inverse(_currentItem.HoldingPoint.rotation) * _currentItem.transform.rotation;
            _currentItem.transform.position = placeToolPoint.transform.position + _currentItem.transform.position - _currentItem.HoldingPoint.position;
            _currentItem.transform.parent = placeToolPoint;

            _currentItem.OnEquip(this);
            _currentItemData = _currentSlot.ItemData;
        }
    }

    public void ConsumeCurrentItem()
    {
        _currentSlot.RemoveFromStack(1);
    }
}
