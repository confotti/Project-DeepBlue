using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using NUnit.Framework;
using System.Collections.Generic;

public class MouseItemData : MonoBehaviour
{
    [SerializeField] public Image itemSprite;
    [SerializeField] public TextMeshProUGUI itemCount;
    [SerializeField] private InventorySlot assignedInventorySlot;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;

    private void Awake()
    {
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void UpdateMouseSlot(InventorySlot invSlot)
    {
        assignedInventorySlot.AssignItem(invSlot);
        itemSprite.sprite = invSlot.ItemData.Icon;
        itemCount.text = invSlot.StackSize > 1 ? invSlot.StackSize.ToString() : "";
        itemSprite.color = Color.white;
    }

    public void UpdateMouseSlotUI()
    {
        if(assignedInventorySlot.ItemData != null) 
        {
            itemSprite.sprite = assignedInventorySlot.ItemData.Icon;
            itemCount.text = assignedInventorySlot.StackSize > 1 ? assignedInventorySlot.StackSize.ToString() : "";
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
        //TODO: Add controller support
        //Follows mouseposition if it has an item
        if(assignedInventorySlot != null)
        {
            transform.position = Mouse.current.position.ReadValue();

            if(Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())
            {
                ClearSlot();
                //TODO: Drop item on the ground instead of deleting it
            }
        }
    }

    //Clears the slot and updates UI. 
    public void ClearSlot()
    {
        assignedInventorySlot.ClearSlot();
        itemCount.text = "";
        itemSprite.color = Color.clear;
        itemSprite.sprite = null;
    }

    //Checks if we are mouse is over UI or not. 
    public static bool IsPointerOverUIObject()
    {
        //TODO: Can probably make it more effective both in memory and time by using Raycast
        //instead of RaycastAll.

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Mouse.current.position.ReadValue();
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
