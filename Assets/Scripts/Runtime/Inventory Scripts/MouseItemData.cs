using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using NUnit.Framework;
using System.Collections.Generic;

public class MouseItemData : MonoBehaviour
{
    public Image itemSprite;
    public TextMeshProUGUI itemCount;
    public InventorySlot AssignedInventorySlot;

    private void Awake()
    {
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        AssignedInventorySlot.AssignItem(invSlot);
        itemSprite.sprite = invSlot.ItemData.icon;
        itemCount.text = invSlot.StackSize > 1 ? invSlot.StackSize.ToString() : "";
        itemSprite.color = Color.white;
    }

    public void UpdateMouseSlotUI()
    {
        if(AssignedInventorySlot.ItemData != null) 
        {
            itemSprite.sprite = AssignedInventorySlot.ItemData.icon;
            itemCount.text = AssignedInventorySlot.StackSize > 1 ? AssignedInventorySlot.StackSize.ToString() : "";
            itemSprite.color = Color.white;
        }
        else
        {
            itemSprite.sprite = null;
            itemSprite.color = Color.clear;
            itemCount.text = "";
        }
    }

    private void Update()
    {
        if(AssignedInventorySlot != null)
        {
            transform.position = Mouse.current.position.ReadValue();

            if(Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())
            {
                ClearSlot();
            }
        }
    }

    public void ClearSlot()
    {
        AssignedInventorySlot.ClearSlot();
        itemCount.text = "";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
    }

    public static bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
