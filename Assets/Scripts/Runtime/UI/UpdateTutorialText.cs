using UnityEngine;
using TMPro;
using WrightAngle.Waypoint;

public class UpdateTutorialText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject firstTutorialUI;
    [SerializeField] private GameObject topBorder;
    [SerializeField] private GameObject bottomBorder;

    [Header("Items")]
    [SerializeField] private InventoryItemData limestone;
    [SerializeField] private InventoryItemData tin;
    [SerializeField] private InventoryItemData hammer;

    [Header("Waypoints")]
    [SerializeField] private WaypointTarget lightSwitchWaypoint;
    [SerializeField] private WaypointTarget resourceWaypoint;
    [SerializeField] private WaypointTarget hammerWaypoint;

    [Header("Light Switch")]
    [SerializeField] private LightBehaviour lightSwitch; // assign in Inspector

    private PlayerInventoryHolder inventory;
    private bool isActive = false;

    private enum TutorialStep
    {
        InteractWithLight,
        CollectResources,
        GetHammer,
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
        if (topBorder != null)
            topBorder.SetActive(true);
        if (bottomBorder != null)
            bottomBorder.SetActive(true);

        // Start with light interaction
        currentStep = TutorialStep.InteractWithLight;

        // Subscribe to light switch event
        if (lightSwitch != null)
            lightSwitch.OnFirstInteract += OnLightInteracted;

        // Activate waypoint above light switch
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

            case TutorialStep.CollectResources:
                HandleCollectResources();
                break;

            case TutorialStep.GetHammer:
                HandleHammerStep();
                break;

            case TutorialStep.Done:
                tutorialText.gameObject.SetActive(false);
                if (topBorder != null) topBorder.SetActive(false);
                if (bottomBorder != null) bottomBorder.SetActive(false);
                break;
        }
    }

    // Called automatically when the light is interacted with
    private void OnLightInteracted()
    {
        if (!isActive || currentStep != TutorialStep.InteractWithLight)
            return;

        currentStep = TutorialStep.CollectResources;

        // Deactivate light switch waypoint
        if (lightSwitchWaypoint != null)
            lightSwitchWaypoint.DeactivateWaypoint();

        // Activate resource collection waypoint
        if (resourceWaypoint != null)
            resourceWaypoint.ActivateWaypoint();

        UpdateText();
    }

    private void HandleCollectResources()
    {
        int limestoneCount = GetItemCount(limestone);
        int tinCount = GetItemCount(tin);

        tutorialText.text = $"Collect: Limestone: {limestoneCount}/4 and Tin: {tinCount}/2";

        if (limestoneCount >= 4 && tinCount >= 2)
        {
            currentStep = TutorialStep.GetHammer;

            // Switch waypoint from resource to hammer
            SwitchWaypoint(resourceWaypoint, hammerWaypoint);
            UpdateText();
        }
    }

    private void HandleHammerStep()
    {
        int hammerCount = GetItemCount(hammer);

        if (hammerCount > 0)
        {
            currentStep = TutorialStep.Done;

            // Deactivate hammer waypoint
            if (hammerWaypoint != null)
                hammerWaypoint.DeactivateWaypoint();

            UpdateText();
            return;
        }

        tutorialText.text = "Pick up the hammer";
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

    // Waypoint helper methods
    private void SwitchWaypoint(WaypointTarget oldTarget, WaypointTarget newTarget)
    {
        if (oldTarget != null)
        {
            oldTarget.ActivateWaypoint(); // Ensure registered
            oldTarget.DeactivateWaypoint();
        }

        if (newTarget != null)
            newTarget.ActivateWaypoint();
    }

    public void OnWorkbenchBuilt()
    {
        if (currentStep == TutorialStep.GetHammer) return; // We removed workbench waypoint

        currentStep = TutorialStep.Done;
        UpdateText();
    }
} 