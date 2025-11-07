using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public DynamicInventoryDisplay inventoryPanel;
    public DynamicInventoryDisplay playerBackpackPanel;
    public CraftingDisplay craftingDisplay;

    private void Awake()
    {
        inventoryPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
        //craftingDisplay.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        //PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
        CraftingDisplay.OnCraftingDisplayRequested += DisplayCraftingWindow;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        //PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;
        CraftingDisplay.OnCraftingDisplayRequested -= DisplayCraftingWindow;
    }

    void Update()
    {
        //TODO: Implement with input action asset
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (inventoryPanel.gameObject.activeInHierarchy) inventoryPanel.gameObject.SetActive(false);

        if (playerBackpackPanel.gameObject.activeInHierarchy) playerBackpackPanel.gameObject.SetActive(false);

        if (craftingDisplay.gameObject.activeInHierarchy) craftingDisplay.gameObject.SetActive(false);
    }

    private void DisplayInventory(InventorySystem invToDisplay, int offset)
    {
        inventoryPanel.gameObject.SetActive(true);
        inventoryPanel.RefreshDynamicInventory(invToDisplay, offset);
    }

    /*
    private void DisplayPlayerBackpack(InventorySystem invToDisplay)
    {
        playerBackpackPanel.gameObject.SetActive(true);
        playerBackpackPanel.RefreshDynamicInventory(invToDisplay);
    }*/

    
    private void DisplayCraftingWindow(CraftingBench craftingToDisplay)
    {
        craftingDisplay.gameObject.SetActive(true);
        craftingDisplay.DisplayCraftingWindow(craftingToDisplay);
    }
    
}
