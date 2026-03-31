using UnityEngine;
using TMPro;
using WrightAngle.Waypoint;
using BuildSystem;
using System.Collections;

public class UpdateTutorialText : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private TMP_Text nightWarningText; 
    [SerializeField] private GameObject firstTutorialUI;
    [SerializeField] private GameObject flashlightUI;

    [Header("Items")]
    [SerializeField] private InventoryItemData limestone;
    [SerializeField] private InventoryItemData tin;
    [SerializeField] private InventoryItemData oxygenTank;
    [SerializeField] private InventoryItemData flashlight;
    [SerializeField] private InventoryItemData hammer;
    [SerializeField] private InventoryItemData copperIngot;
    [SerializeField] private InventoryItemData repairTorch;

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
    [SerializeField] private EquipmentSlot_UI oxygenTankSlotUI;

    [Header("Night Return Settings")]
    [SerializeField] private int returnStartHour = 20;
    [SerializeField] private int returnEndHour = 6;

    [Header("Radio")]
    [SerializeField] private AudioSource radioAudioSource;
    [SerializeField] private int radioHour = 23;
    [SerializeField] private GameObject creatureToSpawn; 

    private bool isRadioPlaying = false;
    private PlayerInventoryHolder inventory;
    private bool isActive = false;

    private bool hasShownFlashlight = false;
    private bool wasSwimmingLastFrame = false;

    private bool flashlightCollected = false;
    private bool hammerCollected = false;
    private bool oxygenTankCollected = false;
    private bool oxygenTankEquipped = false;

    private bool isNightUIActive = false;
    private bool hasShownNightWarning = false; 

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
        CraftRepairTorch,
        Done
    }

    private TutorialStep currentStep;

    private void Awake()
    {
        inventory = FindObjectOfType<PlayerInventoryHolder>();
        if (nightWarningText != null)
            nightWarningText.gameObject.SetActive(false); 
        if (creatureToSpawn != null)
            creatureToSpawn.SetActive(false); 
    } 

    private void Start()
    {
        ActivateTutorial();

        if (PlayerMovement.Instance != null)
            wasSwimmingLastFrame = PlayerMovement.Instance.IsSwimming;

        if (TimeManager.Instance != null)
            UpdateTutorial();
    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += UpdateTutorial;
        BuildTool.OnBuildingPlaced += OnBuildingBuilt;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract += OnLightInteracted;

        if (TimeManager.Instance != null)
            TimeManager.Instance.OnHourChanged += OnHourChanged;
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= UpdateTutorial;
        BuildTool.OnBuildingPlaced -= OnBuildingBuilt;

        if (lightSwitch != null)
            lightSwitch.OnFirstInteract -= OnLightInteracted;

        if (TimeManager.Instance != null)
            TimeManager.Instance.OnHourChanged -= OnHourChanged;
    }

    public void ActivateTutorial()
    {
        isActive = true;
        firstTutorialUI?.SetActive(false);
        tutorialText?.gameObject.SetActive(true);

        TimeManager.Instance?.PauseTime();

        currentStep = TutorialStep.InteractWithLight;
        lightSwitchWaypoint?.ActivateWaypoint();
        UpdateTutorial();
    }

    private void Update()
    {
        if (isActive)
        {
            if (currentStep == TutorialStep.ExitSubmarine)
                CheckExitSubStep();

            CheckFlashlightHint();

            if (currentStep == TutorialStep.EquipOxygenTank && IsOxygenTankInSlot())
            {
                oxygenTankEquipped = true;
                currentStep = TutorialStep.ExitSubmarine;
                UpdateTutorial();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                SkipToNightTutorial();
            }
        }

        CheckNightReturnWarning();
        CheckNightWarning(); 
    }

    private void SkipTutorial()
    {
        currentStep = TutorialStep.Done;
        TimeManager.Instance?.ResumeTime();
        UpdateTutorial();
    }

    private void OnHourChanged(int hour)
    {
        if (currentStep == TutorialStep.Done)
        {
            bool isRadioTime = IsTimeInRange(hour, radioHour, 6);

            if (isRadioTime)
                StartRadioEvent();
            else
                StopRadioEvent();
        }
    }

    private bool IsTimeInRange(int hour, int start, int end)
    {
        if (start > end)
            return hour >= start || hour < end;

        return hour >= start && hour < end;
    }

    private void StartRadioEvent()
    {
        if (isRadioPlaying || radioAudioSource == null) return;

        isRadioPlaying = true;
        radioAudioSource.loop = true;
        radioAudioSource.Play();

        tutorialText.text = "Explore the sound";
    }

    private void StopRadioEvent()
    {
        if (!isRadioPlaying || radioAudioSource == null) return;

        isRadioPlaying = false;
        radioAudioSource.Stop();

        UpdateTutorial();
    }

    private void CheckNightReturnWarning()
    {
        if (TimeManager.Instance == null || PlayerMovement.Instance == null) return;
        if (currentStep != TutorialStep.Done) return;

        int hour = TimeManager.Instance.GetGameTimeStamp().Hour;
        bool isNightTime = IsTimeInRange(hour, returnStartHour, returnEndHour);
        bool isSwimming = PlayerMovement.Instance.IsSwimming;

        bool shouldShow = isNightTime && isSwimming;

        if (shouldShow)
        {
            if (!isNightUIActive)
            {
                isNightUIActive = true;
                tutorialText.text = "Return to Submarine";
            }
        }
        else
        {
            isNightUIActive = false;

            if (!isRadioPlaying)
            {
                tutorialText.text = "";
            }
        }
    }

    private void CheckNightWarning()
    {
        if (TimeManager.Instance == null || hasShownNightWarning) return;

        var time = TimeManager.Instance.GetGameTimeStamp();

        if (time.Hour == 23 && time.Minute == 30)
        {
            hasShownNightWarning = true;
            ShowNightWarning();
        }
    }

    private void ShowNightWarning()
    {
        if (nightWarningText != null)
        {
            nightWarningText.gameObject.SetActive(true);
            nightWarningText.text = "...stay indoors...they come out at night...";
            StartCoroutine(HideNightWarningAfterSeconds(5f));
        }

        if (creatureToSpawn != null)
            creatureToSpawn.SetActive(true); 
    }

    private IEnumerator HideNightWarningAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (nightWarningText != null)
            nightWarningText.gameObject.SetActive(false);
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

    private void SkipToNightTutorial()
    {
        currentStep = TutorialStep.Done;

        TimeManager.Instance?.ResumeTime();

        isNightUIActive = false;
        isRadioPlaying = false;

        CheckNightReturnWarning();
        CheckNightWarning();
        UpdateTutorial();

        Debug.Log("Skipped to night tutorial.");
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
            case TutorialStep.CraftRepairTorch:
                CraftRepairTorch();
                break;
            case TutorialStep.Done:
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
            currentStep = TutorialStep.CraftRepairTorch;
            UpdateTutorial();
        }
    }

    private void CraftRepairTorch()
    {
        int repairTorchCount = GetItemCount(repairTorch);
        tutorialText.text = $"Craft the Repair Torch: {repairTorchCount}/1";

        if (repairTorchCount >= 1)
        {
            TimeManager.Instance?.ResumeTime();
            currentStep = TutorialStep.Done;
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