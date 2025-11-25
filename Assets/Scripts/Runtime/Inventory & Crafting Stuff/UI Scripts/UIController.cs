using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public DynamicInventoryDisplay inventoryPanel;
    public DynamicInventoryDisplay playerInventoryPanel;
    public CraftingDisplay craftingDisplay;
    public BuildDisplay buildDisplay;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        inventoryPanel.gameObject.SetActive(false);
        playerInventoryPanel.gameObject.SetActive(false);
        craftingDisplay.gameObject.SetActive(false);
        buildDisplay.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested += DisplayPlayerInventory;
        CraftingDisplay.OnCraftingDisplayRequested += DisplayCraftingWindow;
        BuildDisplay.OnBuildDisplayRequested += DisplayBuildWindow;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested -= DisplayPlayerInventory;
        CraftingDisplay.OnCraftingDisplayRequested -= DisplayCraftingWindow;
        BuildDisplay.OnBuildDisplayRequested -= DisplayBuildWindow;
    }

    void Update()
    {
        //TODO: Implement with input action asset
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        ToggleLooking(true);

        if (inventoryPanel.gameObject.activeInHierarchy) inventoryPanel.gameObject.SetActive(false);

        if (playerInventoryPanel.gameObject.activeInHierarchy) playerInventoryPanel.gameObject.SetActive(false);

        if (craftingDisplay.gameObject.activeInHierarchy) craftingDisplay.gameObject.SetActive(false);

        if(buildDisplay.gameObject.activeInHierarchy) buildDisplay.gameObject.SetActive(false);
    }

    private void DisplayInventory(InventorySystem invToDisplay, int offset)
    {
        inventoryPanel.gameObject.SetActive(true);
        inventoryPanel.RefreshDynamicInventory(invToDisplay, offset);

        ToggleLooking(false);
    }

    
    private void DisplayPlayerInventory(InventorySystem invToDisplay, int offset)
    {
        playerInventoryPanel.gameObject.SetActive(true);
        playerInventoryPanel.RefreshDynamicInventory(invToDisplay, offset);

        ToggleLooking(false);
    }

    
    private void DisplayCraftingWindow(CraftingBench craftingToDisplay)
    {
        craftingDisplay.gameObject.SetActive(true);
        craftingDisplay.DisplayCraftingWindow(craftingToDisplay);

        ToggleLooking(false);
    }

    private void DisplayBuildWindow()
    {
        buildDisplay.gameObject.SetActive(true);
        buildDisplay.DisplayBuildWindow();

        ToggleLooking(false);
    }

    private void ToggleLooking(bool enabled)
    {
        if (enabled) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
        PlayerInputHandler.ToggleLooking?.Invoke(enabled);
    }
    
}
