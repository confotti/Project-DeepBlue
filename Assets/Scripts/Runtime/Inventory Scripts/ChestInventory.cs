using SaveLoadSystem;
using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueID))]
public class ChestInventory : InventoryHolder, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    protected override void Awake()
    {
        base.Awake();
        SaveLoad.OnLoadGame += LoadInventory;
    }

    private void OnDestroy()
    {
        SaveLoad.OnLoadGame -= LoadInventory;
    }

    private void Start()
    {
        var chestSaveData = new InventorySaveData(inventorySystem, transform.position, transform.rotation);

        if(SaveLoad.currentSavedata != null) SaveLoad.currentSavedata.chestDictionary.Add(GetComponent<UniqueID>().ID, chestSaveData);
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
            this.inventorySystem = chestData.invSystem;
            this.transform.position = chestData.position;
            this.transform.rotation = chestData.rotation;
        }
    }

}

