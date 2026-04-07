using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SubmarineEngine : MonoBehaviour, IInteractable
{
    [Header("Fuel Settings")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float drainRate = 2f;
    [SerializeField] private float warningDrainRate = 0f;

    [Header("Fuel Item")]
    [SerializeField] private InventoryItemData fuelItem;
    [SerializeField] private float fuelPerItem = 25f;
    [SerializeField] private AudioSource audioSource; 

    [Header("References")]
    [SerializeField] private Image fuelFillImage;
    [SerializeField] private LightBehaviour lightBehaviour;
    [SerializeField] private PlayerInventoryHolder playerInventory;

    private float currentFuel;
    private bool warningLightsOnly = true;
    private bool isOutOfFuel = false;

    public string InteractText => "Refill Engine";
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void OnEnable()
    {
        LightBehaviour.OnWarningLightsChanged += HandleWarningLights;
    }

    private void OnDisable()
    {
        LightBehaviour.OnWarningLightsChanged -= HandleWarningLights;
    }

    private void Start()
    {
        currentFuel = maxFuel;
        UpdateUI();
    }

    private void Update()
    {
        DrainFuel();
    }

    public void Interact(PlayerInteract interactor)
    {
        if (currentFuel >= maxFuel)
            return;

        if (playerInventory == null || fuelItem == null)
            return;

        int amount = playerInventory.InventorySystem.AmountOfItem(fuelItem);

        if (amount <= 0)
            return;

        AddFuel(fuelPerItem);
        playerInventory.RemoveItemFromInventory(fuelItem, 1);
        audioSource.Play(); 
    }

    public void EndInteraction() { }

    private void DrainFuel()
    {
        if (currentFuel <= 0.01f)
        {
            currentFuel = 0f;

            if (!isOutOfFuel)
            {
                isOutOfFuel = true;
                ForceWarningLights();
            }

            UpdateUI();
            return;
        }

        float rate = warningLightsOnly ? warningDrainRate : drainRate;

        currentFuel -= rate * Time.deltaTime;
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);

        UpdateUI();
    }

    private void ForceWarningLights()
    {
        if (lightBehaviour != null)
        {
            lightBehaviour.ForceWarningLights();
        }
    }

    private void UpdateUI()
    {
        if (fuelFillImage != null)
        {
            fuelFillImage.fillAmount = currentFuel / maxFuel;
        }
    }

    private void HandleWarningLights(bool warningOnly)
    {
        warningLightsOnly = warningOnly;
    }

    public void AddFuel(float amount)
    {
        currentFuel += amount;
        currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);

        if (currentFuel > 0f)
        {
            isOutOfFuel = false;
        }

        UpdateUI();
    }

    public bool HasFuel()
    {
        return currentFuel > 0.01f;
    }
} 