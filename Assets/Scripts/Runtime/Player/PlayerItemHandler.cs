using UnityEngine;

public class PlayerItemHandler : MonoBehaviour
{
    private ItemBehaviour _currentItem;

    private PlayerInputHandler _playerInputs;

    [SerializeField] private Transform _playerHead;

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
            Destroy(_currentItem.gameObject);
            _currentItem = null;
        } 

        if (newItem != null)
        {
            _currentItem = Instantiate(newItem, gameObject.transform);
            _currentItem.Init(_playerHead);
        } 
    }
}
