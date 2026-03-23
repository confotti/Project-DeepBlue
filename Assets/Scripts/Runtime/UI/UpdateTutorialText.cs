using UnityEngine;
using TMPro;
using WrightAngle.Waypoint;

public class UpdateTutorialText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject firstTutorialUI;

    [Header("Items")]
    [SerializeField] private InventoryItemData limestone;
    [SerializeField] private InventoryItemData tin;
    [SerializeField] private InventoryItemData oxygenTank;
    [SerializeField] private InventoryItemData flashlight;
    [SerializeField] private InventoryItemData hammer;

    [Header("Waypoints")]
    [SerializeField] private WaypointTarget lightSwitchWaypoint;
    [SerializeField] private WaypointTarget flashlighthWaypoint;
    [SerializeField] private WaypointTarget oxygenTankWaypoint;
    [SerializeField] private WaypointTarget resourceWaypoint;
    [SerializeField] private WaypointTarget hammerWaypoint;

    [Header("Light Switch")]
    [SerializeField] private LightBehaviour lightSwitch;

    private PlayerInventoryHolder inventory;
    private bool isActive = false;

    private enum TutorialStep
    {
        InteractWithLight,
        CollectGear,
        CollectResources,
        Done
    }

    private TutorialStep currentStep;

    private void Awake()
    {
        inventory = FindObjectOfType<PlayerInventoryHolder>();
    }

    private void Start()
    {
        ActivateTutorial();
    }

    public void ActivateTutorial()
    {
        isActive = true;

        if (firstTutorialUI != null)
            firstTutorialUI.SetActive(false);

        if (tutorialText != null)
            tutorialText.gameObject.SetActive(true);

        currentStep = TutorialStep.InteractWithLight;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract += OnLightInteracted;

        if (lightSwitchWaypoint != null)
            lightSwitchWaypoint.ActivateWaypoint();

        UpdateText();
    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += UpdateText;
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= UpdateText;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract -= OnLightInteracted;
    }

    private void UpdateText()
    {
        if (!isActive || tutorialText == null) return;

        switch (currentStep)
        {
            case TutorialStep.InteractWithLight:
                tutorialText.text = "Reset the engine"; 
                break;

            case TutorialStep.CollectGear:
                HandleCollectGear();
                break;

            case TutorialStep.CollectResources:
                HandleCollectResources();
                break;

            case TutorialStep.Done:
                tutorialText.gameObject.SetActive(false);
                break;
        }
    }

    private void OnLightInteracted()
    {
        if (!isActive || currentStep != TutorialStep.InteractWithLight)
            return;

        currentStep = TutorialStep.CollectGear;

        // Disable light waypoint
        if (lightSwitchWaypoint != null)
            lightSwitchWaypoint.DeactivateWaypoint();

        // Activate ALL gear waypoints
        if (flashlighthWaypoint != null)
            flashlighthWaypoint.ActivateWaypoint();

        if (oxygenTankWaypoint != null)
            oxygenTankWaypoint.ActivateWaypoint();

        if (hammerWaypoint != null)
            hammerWaypoint.ActivateWaypoint();

        UpdateText();
    }

    private void HandleCollectGear()
    {
        int flashlightCount = GetItemCount(flashlight);
        int hammerCount = GetItemCount(hammer);
        int oxygenTankCount = GetItemCount(oxygenTank);

        tutorialText.text =
            $"Collect and equip your gear:\nFlashlight: {flashlightCount}/1\nHammer: {hammerCount}/1\nOxygen Tank: {oxygenTankCount}/1";

        // Disable waypoint when collected
        if (flashlightCount >= 1 && flashlighthWaypoint != null)
            flashlighthWaypoint.DeactivateWaypoint();

        if (hammerCount >= 1 && hammerWaypoint != null)
            hammerWaypoint.DeactivateWaypoint();

        if (oxygenTankCount >= 1 && oxygenTankWaypoint != null)
            oxygenTankWaypoint.DeactivateWaypoint();

        // Move to next step when all collected
        if (flashlightCount >= 1 && hammerCount >= 1 && oxygenTankCount >= 1)
        {
            currentStep = TutorialStep.CollectResources;

            if (resourceWaypoint != null)
                resourceWaypoint.ActivateWaypoint();

            UpdateText();
        }
    }

    private void HandleCollectResources()
    {
        int limestoneCount = GetItemCount(limestone);
        int tinCount = GetItemCount(tin);

        tutorialText.text =
            $"Collect resources:\nLimestone: {limestoneCount}/4\nTin: {tinCount}/2";

        if (limestoneCount >= 4 && tinCount >= 2)
        {
            currentStep = TutorialStep.Done;
            UpdateText();
        }
    }

    private int GetItemCount(InventoryItemData item)
    {
        int count = 0;
        if (inventory == null) return 0;

        foreach (var slot in inventory.InventorySystem.InventorySlots)
        {
            if (slot.ItemData == item)
                count += slot.StackSize;
        }

        return count;
    }

    public void OnWorkbenchBuilt()
    {
        if (!isActive) return;

        if (currentStep == TutorialStep.CollectResources)
        {
            currentStep = TutorialStep.Done;
            UpdateText();
        }
    } 
} 