using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot_UI : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;
    [SerializeField] private InventorySlot assignedInventorySlot;

    private Button button;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay { get; private set; }

    private void OnValidate()
    {
        UpdateUISlot();
    }

    private void Awake()
    {
        ClearSlot();

        button = GetComponent<Button>();
        button?.onClick.AddListener(OnUISlotClick);

        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
    }

    private void OnDestroy()
    {
        button?.onClick.RemoveListener(OnUISlotClick);
    }

    public void Init(InventorySlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot)
    {
        if(slot.ItemData != null)
        {
            itemSprite.sprite = slot.ItemData.icon;
            itemSprite.color = Color.white;

            if (slot.StackSize > 1) itemCount.text = slot.StackSize.ToString();
            else itemCount.text = "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void UpdateUISlot()
    {
        if(assignedInventorySlot != null) UpdateUISlot(assignedInventorySlot);
        else ClearSlot();
    }

    //Clears out the slot and updates UI. 
    public void ClearSlot()
    {
        assignedInventorySlot.ClearSlot();
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void OnUISlotClick()
    {
        ParentDisplay?.SlotClicked(this);
    }
}
