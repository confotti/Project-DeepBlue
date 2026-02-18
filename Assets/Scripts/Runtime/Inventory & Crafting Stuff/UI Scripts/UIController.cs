using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public DynamicInventoryDisplay DynamicInventoryPanel;
    public DynamicInventoryDisplay PlayerInventoryPanel;
    public GameObject EquipmentPanel;
    public CraftingDisplay CraftingDisplay;
    public BuildDisplay BuildDisplay;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        DynamicInventoryPanel.gameObject.SetActive(false);
        PlayerInventoryPanel.gameObject.SetActive(false);
        CraftingDisplay.gameObject.SetActive(false);
        BuildDisplay.gameObject.SetActive(false);
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

        DynamicInventoryPanel.gameObject.SetActive(false);

        PlayerInventoryPanel.gameObject.SetActive(false);
        
        EquipmentPanel.SetActive(false);

        CraftingDisplay.gameObject.SetActive(false);

        BuildDisplay.gameObject.SetActive(false);
    }

    private void DisplayInventory(InventorySystem invToDisplay, int offset)
    {
        DynamicInventoryPanel.gameObject.SetActive(true);
        DynamicInventoryPanel.RefreshDynamicInventory(invToDisplay, offset);

        ToggleLooking(false);
    }

    
    private void DisplayPlayerInventory(InventorySystem invToDisplay, int offset)
    {
        PlayerInventoryPanel.gameObject.SetActive(true);
        PlayerInventoryPanel.RefreshDynamicInventory(invToDisplay, offset);
        EquipmentPanel.SetActive(true);

        ToggleLooking(false);
    }

    
    private void DisplayCraftingWindow(CraftingBench craftingToDisplay)
    {
        CraftingDisplay.gameObject.SetActive(true);
        CraftingDisplay.DisplayCraftingWindow(craftingToDisplay);

        ToggleLooking(false);
    }

    private void DisplayBuildWindow()
    {
        BuildDisplay.gameObject.SetActive(true);
        BuildDisplay.DisplayBuildWindow();

        ToggleLooking(false);
    }

    private void ToggleLooking(bool enabled)
    {
        if (enabled) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
        PlayerInputHandler.ToggleLooking?.Invoke(enabled);
    }
    
}
