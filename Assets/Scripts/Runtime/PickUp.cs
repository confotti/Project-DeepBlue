using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class LootEntry
{
    public GameObject item;
    [Tooltip("Probability weight for this item")]
    public float weight = 1f;
}

public class PickUp : MonoBehaviour, IInteractable
{
    public GameObject holder;
    public Transform pos;

    [Tooltip("Assign the loot items with their probability weights here")]
    public LootEntry[] lootTable;

    public string InteractText => "Press E to pick up";

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void EndInteraction() { }

    public void Interact(PlayerInteract interactor)
    {
        if (lootTable == null || lootTable.Length == 0) return;

        int index = GetWeightedRandomIndex();
        if (index >= 0 && lootTable[index].item != null)
            Instantiate(lootTable[index].item, pos.position, Quaternion.identity);

        holder.SetActive(false);
    }

    private int GetWeightedRandomIndex()
    {
        float totalWeight = 0f;
        foreach (var entry in lootTable)
            totalWeight += entry.weight;

        float randomValue = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < lootTable.Length; i++)
        {
            cumulative += lootTable[i].weight;
            if (randomValue <= cumulative)
                return i;
        }

        return lootTable.Length - 1; // fallback
    }
} 