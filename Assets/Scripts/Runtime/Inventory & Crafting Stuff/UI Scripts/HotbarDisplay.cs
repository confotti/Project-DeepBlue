using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarDisplay : StaticInventoryDisplay
{
    private int _maxIndexSize = 9;
    private int _currentIndex = 0;

    protected override void Start()
    {
        base.Start();

        _currentIndex = 0;
        _maxIndexSize = slots.Length - 1;

        slots[_currentIndex].ToggleHighlight();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        //Inputs are here
        //Button 1-0, scrollwheel and use item.
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        //Unsubscribe inputs
    }

    private void Hotbar1(InputAction.CallbackContext context)
    {
        SetIndex(0);
    }

    void Update()
    {
        //if(mouseWheelInput > 0.1f) ChangeIndex(1);
        //if(mouseWheelInput < -0.1f) ChangeIndex(-1);
    }

    private void UseItem(InputAction.CallbackContext context)
    {
        if (slots[_currentIndex].AssignedInventorySlot.ItemData != null)
        {
            slots[_currentIndex].AssignedInventorySlot.ItemData.UseItem();
        }
    }

    private void ChangeIndex(int direction)
    {
        slots[_currentIndex].ToggleHighlight();
        _currentIndex += direction;

        if (_currentIndex > _maxIndexSize) _currentIndex -= _maxIndexSize + 1;
        if(_currentIndex < 0) _currentIndex += _maxIndexSize + 1;

        slots[_currentIndex].ToggleHighlight();
    }

    private void SetIndex(int newIndex)
    {
        if (newIndex == _currentIndex) return;
        slots[_currentIndex].ToggleHighlight();

        if (_currentIndex > _maxIndexSize) newIndex = _maxIndexSize;
        if (_currentIndex < 0) newIndex = 0;

        _currentIndex = newIndex;
        slots[_currentIndex].ToggleHighlight();
    }


}
