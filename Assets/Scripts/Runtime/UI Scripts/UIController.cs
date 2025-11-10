using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public DynamicInventoryDisplay inventoryPanel;
    public DynamicInventoryDisplay playerInventoryPanel;
    public CraftingDisplay craftingDisplay;

    private void Awake()
    {
        inventoryPanel.gameObject.SetActive(false);
        playerInventoryPanel.gameObject.SetActive(false);
        craftingDisplay.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested += DisplayPlayerInventory;
        CraftingDisplay.OnCraftingDisplayRequested += DisplayCraftingWindow;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested -= DisplayPlayerInventory;
        CraftingDisplay.OnCraftingDisplayRequested -= DisplayCraftingWindow;
    }

    void Update()
    {
        //TODO: Implement with input action asset
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if (inventoryPanel.gameObject.activeInHierarchy) inventoryPanel.gameObject.SetActive(false);

        if (playerInventoryPanel.gameObject.activeInHierarchy) playerInventoryPanel.gameObject.SetActive(false);

        if (craftingDisplay.gameObject.activeInHierarchy) craftingDisplay.gameObject.SetActive(false);
    }

    private void DisplayInventory(InventorySystem invToDisplay, int offset)
    {
        inventoryPanel.gameObject.SetActive(true);
        inventoryPanel.RefreshDynamicInventory(invToDisplay, offset);
    }

    
    private void DisplayPlayerInventory(InventorySystem invToDisplay, int offset)
    {
        playerInventoryPanel.gameObject.SetActive(true);
        playerInventoryPanel.RefreshDynamicInventory(invToDisplay, offset);
    }

    
    private void DisplayCraftingWindow(CraftingBench craftingToDisplay)
    {
        craftingDisplay.gameObject.SetActive(true);
        craftingDisplay.DisplayCraftingWindow(craftingToDisplay);
    }
    
}
