using UnityEngine;
using TMPro;
using WrightAngle.Waypoint;
using BuildSystem;
using System.Collections;

public class UpdateTutorialText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private GameObject firstTutorialUI;
    [SerializeField] private GameObject flashlightUI;

    [Header("Items")]
    [SerializeField] private InventoryItemData limestone;
    [SerializeField] private InventoryItemData tin;
    [SerializeField] private InventoryItemData oxygenTank;
    [SerializeField] private InventoryItemData flashlight;
    [SerializeField] private InventoryItemData hammer;
    [SerializeField] private InventoryItemData copperIngot; 

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

    [Header("Oxygen Tank Slot")]
    [SerializeField] private EquipmentSlot_UI oxygenTankSlotUI; // The specific UI slot to equip

    private PlayerInventoryHolder inventory;
    private bool isActive = false;

    private bool hasShownFlashlight = false;
    private bool wasSwimmingLastFrame = false;

    private bool flashlightCollected = false;
    private bool hammerCollected = false;
    private bool oxygenTankCollected = false;
    private bool oxygenTankEquipped = false;

    private enum TutorialStep
    {
        InteractWithLight,
        CollectGear,
        EquipOxygenTank,
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
        if (PlayerMovement.Instance != null)
            wasSwimmingLastFrame = PlayerMovement.Instance.IsSwimming;
    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += UpdateTutorial;
        BuildTool.OnBuildingPlaced += OnBuildingBuilt;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract += OnLightInteracted;
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= UpdateTutorial;
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
        UpdateTutorial();
    }

    private void Update()
    {
        if (!isActive) return;

        // Step to exit submarine
        if (currentStep == TutorialStep.ExitSubmarine)
            CheckExitSubStep();

        CheckFlashlightHint();

        // Step to equip oxygen tank
        if (currentStep == TutorialStep.EquipOxygenTank && IsOxygenTankInSlot())
        {
            oxygenTankEquipped = true;
            currentStep = TutorialStep.ExitSubmarine;
            UpdateTutorial();
        }
    }

    private void CheckFlashlightHint()
    {
        if (hasShownFlashlight || PlayerMovement.Instance == null) return;

        bool isSwimming = PlayerMovement.Instance.IsSwimming;

        if (!wasSwimmingLastFrame && isSwimming && GetItemCount(flashlight) > 0)
        {
            StartCoroutine(ShowFlashlightHint());
            hasShownFlashlight = true;
        }

        wasSwimmingLastFrame = isSwimming;
    }

    private IEnumerator ShowFlashlightHint()
    {
        if (flashlightUI == null) yield break;

        flashlightUI.SetActive(true);
        yield return new WaitForSeconds(5f);
        flashlightUI.SetActive(false);
    }

    private void UpdateTutorial()
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

            case TutorialStep.EquipOxygenTank:
                tutorialText.text = "Equip the Oxygen Tank\nPress (B) to open Inventory";
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
                CollectCopperIngots();
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
        if (!isActive || currentStep != TutorialStep.InteractWithLight) return;

        currentStep = TutorialStep.CollectGear;

        lightSwitchWaypoint?.DeactivateWaypoint();
        flashlighthWaypoint?.ActivateWaypoint();
        hammerWaypoint?.ActivateWaypoint();
        oxygenTankWaypoint?.ActivateWaypoint();

        UpdateTutorial();
    }

    private void HandleCollectGear()
    {
        flashlightCollected |= GetItemCount(flashlight) > 0;
        hammerCollected |= GetItemCount(hammer) > 0;
        oxygenTankCollected |= GetItemCount(oxygenTank) > 0;

        tutorialText.text =
            $"Collect your gear:\n" +
            $"Flashlight: {(flashlightCollected ? 1 : 0)}/1\n" +
            $"Hammer: {(hammerCollected ? 1 : 0)}/1\n" +
            $"Oxygen Tank: {(oxygenTankCollected ? 1 : 0)}/1";

        if (flashlightCollected) flashlighthWaypoint?.DeactivateWaypoint();
        if (hammerCollected) hammerWaypoint?.DeactivateWaypoint();
        if (oxygenTankCollected) oxygenTankWaypoint?.DeactivateWaypoint();

        // Move to next step only after collecting all gear
        if (flashlightCollected && hammerCollected && oxygenTankCollected)
        {
            currentStep = TutorialStep.EquipOxygenTank;
            UpdateTutorial();
        }
    }

    private bool IsOxygenTankInSlot()
    {
        if (oxygenTankSlotUI == null) return false;
        if (oxygenTankSlotUI.AssignedInventorySlot == null) return false;

        return oxygenTankSlotUI.AssignedInventorySlot.ItemData == oxygenTank;
    }

    private void CheckExitSubStep()
    {
        if (PlayerMovement.Instance == null) return;

        if (PlayerMovement.Instance.IsSwimming)
        {
            currentStep = TutorialStep.CollectMineral;
            resourceWaypoint?.ActivateWaypoint();
            UpdateTutorial();
        }
    }

    private void HandleCollectMineral()
    {
        int limestoneCount = GetItemCount(limestone);
        tutorialText.text = $"Collect resources:\nLimestone: {limestoneCount}/1";

        if (limestoneCount >= 1)
        {
            currentStep = TutorialStep.CollectResources;
            UpdateTutorial();
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
            UpdateTutorial();
        }
    }

    private void OnBuildingBuilt(BuildingData builtData)
    {
        if (!isActive) return;

        if (currentStep == TutorialStep.BuildWorkbench && builtData == workbenchData)
        {
            currentStep = TutorialStep.BuildSmelter;
            UpdateTutorial();
            return;
        }

        if (currentStep == TutorialStep.BuildSmelter && builtData == smelterData)
        {
            currentStep = TutorialStep.CollectCopperIngot;
            UpdateTutorial();
            return;
        }
    }

    private void CollectCopperIngots()
    {
        int copperIngotCount = GetItemCount(copperIngot);
        tutorialText.text = $"Collect Copper Ingots: {copperIngotCount}/2";

        if (copperIngotCount >= 2)
        {
            currentStep = TutorialStep.UpgradeWorkbench;
            UpdateTutorial();
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