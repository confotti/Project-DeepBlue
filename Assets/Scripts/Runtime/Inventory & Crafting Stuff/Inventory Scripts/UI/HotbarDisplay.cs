using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarDisplay : StaticInventoryDisplay
{
    //public static Action<ItemBehaviour> EquipNewItem;
    public static Action<InventorySlot> EquipNewSlot;

    private int _maxIndexSize = 9;
    private int _currentIndex = 0;

    protected override void Start()
    {
        base.Start();

        _currentIndex = 0;
        _maxIndexSize = slots.Length - 1;

        slots[_currentIndex].ToggleHighlight();

        Equip();
        inventorySystem.OnInventorySlotChanged += OnInventorySlotChanged;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        PlayerInputHandler.OnHotbarSelection += HotbarSelection;
        PlayerInputHandler.OnHotbarChange += ChangeIndex;

        //Inputs are here
        //Button 1-0, scrollwheel and use item.
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        //Unsubscribe inputs
        PlayerInputHandler.OnHotbarSelection -= HotbarSelection;
        PlayerInputHandler.OnHotbarChange -= ChangeIndex;

    }

    private void OnDestroy()
    {
        inventorySystem.OnInventorySlotChanged -= OnInventorySlotChanged;
    }

    private void HotbarSelection(int slot)
    {
        SetIndex(slot - 1);
    }

    void Update()
    {
        //if(mouseWheelInput > 0.1f) ChangeIndex(1);
        //if(mouseWheelInput < -0.1f) ChangeIndex(-1);
    }

    private void ChangeIndex(int direction)
    {
        slots[_currentIndex].ToggleHighlight();
        _currentIndex += direction;

        if (_currentIndex > _maxIndexSize) _currentIndex -= _maxIndexSize + 1;
        if(_currentIndex < 0) _currentIndex += _maxIndexSize + 1;

        slots[_currentIndex].ToggleHighlight();
        Equip();
    }

    private void SetIndex(int newIndex)
    {
        if (newIndex == _currentIndex) return;
        slots[_currentIndex].ToggleHighlight();

        if (_currentIndex > _maxIndexSize) newIndex = _maxIndexSize;
        if (_currentIndex < 0) newIndex = 0;

        _currentIndex = newIndex;
        slots[_currentIndex].ToggleHighlight();
        Equip();
    }

    private void OnInventorySlotChanged(InventorySlot slotChanged)
    {
        if (slotChanged == slots[_currentIndex].AssignedInventorySlot)
        {
            Equip();
        }
    }

    private void Equip()
    {
        //if(slots[_currentIndex].AssignedInventorySlot.ItemData == null) EquipNewItem?.Invoke(null);
        //else EquipNewItem?.Invoke(slots[_currentIndex].AssignedInventorySlot.ItemData.itemPrefab);

        EquipNewSlot?.Invoke(slots[_currentIndex].AssignedInventorySlot);
    }
}
