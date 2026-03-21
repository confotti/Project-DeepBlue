using UnityEngine;
using TMPro;

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

    private PlayerInventoryHolder inventory;

    private bool isActive = false;

    private enum TutorialStep
    {
        CollectResources,
        GetHammer,
        BuildWorkbench,
        Done
    }

    private TutorialStep currentStep;

    private void Awake()
    {
        inventory = FindObjectOfType<PlayerInventoryHolder>();
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

        currentStep = TutorialStep.CollectResources;
        UpdateText();
    }

    private void OnEnable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged += UpdateText;
    }

    private void OnDisable()
    {
        PlayerInventoryHolder.OnPlayerInventoryChanged -= UpdateText;
    }

    private void UpdateText()
    {
        if (!isActive || tutorialText == null) return;

        switch (currentStep)
        {
            case TutorialStep.CollectResources:
                HandleCollectResources();
                break;

            case TutorialStep.GetHammer:
                HandleHammerStep();
                break;

            case TutorialStep.BuildWorkbench:
                tutorialText.text = "Craft a workbench";
                break;

            case TutorialStep.Done:
                tutorialText.gameObject.SetActive(false);
                if (topBorder != null) topBorder.SetActive(false);
                if (bottomBorder != null) bottomBorder.SetActive(false);
                break;
        }
    }

    private void HandleCollectResources()
    {
        int limestoneCount = GetItemCount(limestone);
        int tinCount = GetItemCount(tin);

        tutorialText.text = $"Collect: Limestone: {limestoneCount}/4 and Tin: {tinCount}/2";

        if (limestoneCount >= 4 && tinCount >= 2)
        {
            currentStep = TutorialStep.GetHammer;
            UpdateText(); 
        }
    }

    private void HandleHammerStep()
    {
        int hammerCount = GetItemCount(hammer);

        if (hammerCount > 0)
        {
            currentStep = TutorialStep.BuildWorkbench;
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

    public void OnWorkbenchBuilt()
    {
        if (currentStep == TutorialStep.BuildWorkbench)
        {
            currentStep = TutorialStep.Done;
            UpdateText(); 
        }
    }
} 