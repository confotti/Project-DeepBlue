using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public DynamicInventoryDisplay DynamicInventoryPanel;
    public DynamicInventoryDisplay PlayerInventoryPanel;
    public GameObject EquipmentPanel;
    public CraftingDisplay CraftingDisplay;
    public BuildDisplay BuildDisplay;
    public EscapeMenu EscapeMenu;

    private bool AnyUIOpen => DynamicInventoryPanel.gameObject.activeSelf || PlayerInventoryPanel.gameObject.activeSelf || 
        EquipmentPanel.gameObject.activeSelf || CraftingDisplay.gameObject.activeSelf || BuildDisplay.gameObject.activeSelf ||
        EscapeMenu.gameObject.activeSelf;

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

        EscapeMenu.OnContinuePressed += EscapeContinuePressed;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerInventoryDisplayRequested -= DisplayPlayerInventory;
        CraftingDisplay.OnCraftingDisplayRequested -= DisplayCraftingWindow;
        BuildDisplay.OnBuildDisplayRequested -= DisplayBuildWindow;

        EscapeMenu.OnContinuePressed -= EscapeContinuePressed;
    }

    void Update()
    {
        //TODO: Implement with input action asset
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        if(AnyUIOpen) CloseAllUI();
        else
        {
            EscapeMenu.gameObject.SetActive(true);
            UpdateLookingState();
        }
    }

    private void DisplayInventory(InventorySystem invToDisplay, int offset)
    {
        DynamicInventoryPanel.gameObject.SetActive(true);
        DynamicInventoryPanel.RefreshDynamicInventory(invToDisplay, offset);

        UpdateLookingState();
    }

    
    private void DisplayPlayerInventory(InventorySystem invToDisplay, int offset)
    {
        PlayerInventoryPanel.gameObject.SetActive(true);
        PlayerInventoryPanel.RefreshDynamicInventory(invToDisplay, offset);
        EquipmentPanel.SetActive(true);

        UpdateLookingState();
    }

    
    private void DisplayCraftingWindow(CraftingBench craftingToDisplay)
    {
        CraftingDisplay.gameObject.SetActive(true);
        CraftingDisplay.DisplayCraftingWindow(craftingToDisplay);

        UpdateLookingState();
    }

    private void DisplayBuildWindow()
    {
        BuildDisplay.gameObject.SetActive(true);
        BuildDisplay.DisplayBuildWindow();

        UpdateLookingState();
    }

    private void UpdateLookingState()
    {
        if (!AnyUIOpen) Cursor.lockState = CursorLockMode.Locked;
        else Cursor.lockState = CursorLockMode.None;
        PlayerInputHandler.ToggleLooking?.Invoke(!AnyUIOpen);
    }

    private void CloseAllUI()
    {
        DynamicInventoryPanel.gameObject.SetActive(false);
        PlayerInventoryPanel.gameObject.SetActive(false);
        EquipmentPanel.SetActive(false);
        CraftingDisplay.gameObject.SetActive(false);
        BuildDisplay.gameObject.SetActive(false);
        EscapeMenu.gameObject.SetActive(false);

        UpdateLookingState();
    }

    private void EscapeContinuePressed()
    {
        EscapeMenu.gameObject.SetActive(false);

        UpdateLookingState();
    }
    
}
