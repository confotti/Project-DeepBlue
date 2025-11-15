using SaveLoadSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueID))]
public class ChestInventory : InventoryHolder, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    [NonSerialized] public bool isChildOfSub;

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.OnSaveGame += SaveInventory;
        SaveLoad.OnLoadGame += LoadInventory;
    }

    private void OnDestroy()
    {
        SaveLoad.OnSaveGame -= SaveInventory;
        SaveLoad.OnLoadGame -= LoadInventory;
    }

    private void Start()
    {

    }

    public void Interact(PlayerInteract interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(inventorySystem, 0);
        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        
    }

    protected override void LoadInventory(SaveData data)
    {
        if(data.chestDictionary.TryGetValue(GetComponent<UniqueID>().ID, out InventorySaveData chestData))    
        {
            LoadFromSaveData(chestData);
            this.transform.position = chestData.position;
            this.transform.rotation = chestData.rotation;
        }
    }

    protected override void SaveInventory()
    {
        SaveLoad.currentSavedata.chestDictionary.Remove(GetComponent<UniqueID>().ID);
        SaveLoad.currentSavedata.chestDictionary[GetComponent<UniqueID>().ID] = new InventorySaveData()
        {
            slots = InventoryToSaveData(),
            position = this.transform.position,
            rotation = this.transform.rotation,
            childOfSub = isChildOfSub
        };
    }
}

