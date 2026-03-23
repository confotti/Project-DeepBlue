using UnityEngine;
using TMPro;
using WrightAngle.Waypoint;
using BuildSystem;

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

    [Header("Buildings")]
    [SerializeField] private BuildingData workbenchData;
    [SerializeField] private BuildingData smelterData; 

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

    private bool flashlightCollected = false;
    private bool hammerCollected = false;
    private bool oxygenTankCollected = false;

    private enum TutorialStep
    {
        InteractWithLight,
        CollectGear,
        ExitSubmarine,
        CollectMineral,
        CollectResources,
        BuildWorkbench,
        BuildSmelter,
        CollectCopperIngot,
        UpgradeWorkbench,
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

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += UpdateText;
        BuildTool.OnBuildingPlaced += OnBuildingBuilt;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract += OnLightInteracted;
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= UpdateText;
        BuildTool.OnBuildingPlaced -= OnBuildingBuilt;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract -= OnLightInteracted;
    }

    public void ActivateTutorial()
    {
        isActive = true;

        firstTutorialUI?.SetActive(false);
        tutorialText?.gameObject.SetActive(true);

        currentStep = TutorialStep.InteractWithLight;

        lightSwitchWaypoint?.ActivateWaypoint();

        UpdateText();
    }

    private void Update()
    {
        if (isActive && currentStep == TutorialStep.ExitSubmarine)
        {
            CheckExitSubStep();
        }
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

            case TutorialStep.ExitSubmarine:
                tutorialText.text = "Exit the submarine";
                break;

            case TutorialStep.CollectMineral:
                HandleCollectMineral();
                break;

            case TutorialStep.CollectResources:
                HandleCollectResources();
                break;

            case TutorialStep.BuildWorkbench:
                tutorialText.text = "Build a Workbench";
                break;

            case TutorialStep.BuildSmelter:
                tutorialText.text = "Build a Smelter";
                break;

            case TutorialStep.CollectCopperIngot:
                tutorialText.text = "Collect a Copper Ingot";
                break;

            case TutorialStep.UpgradeWorkbench:
                tutorialText.text = "Upgrade the Workbench";
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

        lightSwitchWaypoint?.DeactivateWaypoint();

        flashlighthWaypoint?.ActivateWaypoint();
        oxygenTankWaypoint?.ActivateWaypoint();
        hammerWaypoint?.ActivateWaypoint();

        UpdateText();
    }

    private void HandleCollectGear()
    {
        flashlightCollected |= GetItemCount(flashlight) > 0;
        hammerCollected |= GetItemCount(hammer) > 0;
        oxygenTankCollected |= GetItemCount(oxygenTank) > 0;

        tutorialText.text =
            $"Collect and equip your gear:\nFlashlight: {(flashlightCollected ? 1 : 0)}/1\n" +
            $"Hammer: {(hammerCollected ? 1 : 0)}/1\n" +
            $"Oxygen Tank: {(oxygenTankCollected ? 1 : 0)}/1";

        if (flashlightCollected) flashlighthWaypoint?.DeactivateWaypoint();
        if (hammerCollected) hammerWaypoint?.DeactivateWaypoint();
        if (oxygenTankCollected) oxygenTankWaypoint?.DeactivateWaypoint();

        if (flashlightCollected && hammerCollected && oxygenTankCollected)
        {
            currentStep = TutorialStep.ExitSubmarine;
            UpdateText();
        }
    }

    private void CheckExitSubStep()
    {
        if (PlayerMovement.Instance == null) return;

        if (PlayerMovement.Instance.IsSwimming)
        {
            currentStep = TutorialStep.CollectMineral;
            resourceWaypoint?.ActivateWaypoint();
            UpdateText();
        }
    }

    private void HandleCollectMineral()
    {
        int limestoneCount = GetItemCount(limestone);

        tutorialText.text = $"Collect resources:\nLimestone: {limestoneCount}/1";

        if (limestoneCount >= 1)
        {
            currentStep = TutorialStep.CollectResources;
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
            currentStep = TutorialStep.BuildWorkbench;
            UpdateText();
        }
    }

    private void OnBuildingBuilt(BuildingData builtData)
    {
        if (!isActive) return;

        // 🔨 Workbench step → Smelter step
        if (currentStep == TutorialStep.BuildWorkbench && builtData == workbenchData)
        {
            currentStep = TutorialStep.BuildSmelter;
            UpdateText();
            return;
        }

        // 🔥 Smelter step → Next step
        if (currentStep == TutorialStep.BuildSmelter && builtData == smelterData)
        {
            currentStep = TutorialStep.CollectCopperIngot;
            UpdateText();
            return;
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
} 