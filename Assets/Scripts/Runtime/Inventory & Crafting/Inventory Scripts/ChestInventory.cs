using SaveLoadSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueID))]
public class ChestInventory : InventoryHolder, IInteractable
{
    [SerializeField] private string interactText = "";
    public string InteractText => interactText;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    [NonSerialized] public bool isChildOfSub;

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.OnSaveGame += SaveInventory;
        SaveLoad.OnLoadGame += LoadInventory;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        SaveLoad.OnSaveGame -= SaveInventory;
        SaveLoad.OnLoadGame -= LoadInventory;
    }

    private void Start()
    {

    }

    public void Interact(PlayerInteract interactor)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(inventorySystem, 0);
    }

    public void EndInteraction()
    {
        
    }

    protected override void LoadInventory(SaveData data)
    {
        if(data.chestDictionary.TryGetValue(GetComponent<UniqueID>().ID, out InventorySaveData chestData))    
        {
            LoadFromSaveData(chestData);
            transform.position = chestData.position;
            transform.rotation = chestData.rotation;
        }
    }

    protected override void SaveInventory()
    {
        SaveLoad.currentSavedata.chestDictionary.Remove(GetComponent<UniqueID>().ID);
        SaveLoad.currentSavedata.chestDictionary[GetComponent<UniqueID>().ID] = new InventorySaveData()
        {
            slots = InventoryToSaveData(),
            position = transform.position,
            rotation = transform.rotation,
            childOfSub = isChildOfSub
        };
    }
}

