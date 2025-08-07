using UnityEngine;

public class StemSpawner : MonoBehaviour
{
    public GameObject prefab;           // The prefab to spawn
    public int maxInstances = 1000;     // Maximum number of instances (safety limit)
    public float minSpacing = 0.5f;     // Minimum vertical spacing
    public float maxSpacing = 2f;       // Maximum vertical spacing
    public float minHeight = 50f;       // Minimum stem height limit
    public float maxHeight = 100f;      // Maximum stem height limit

    private float currentHeightLimit;
    private float currentY;

    void Start()
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned.");
            return;
        }

        currentHeightLimit = Random.Range(minHeight, maxHeight);

        Vector3 basePosition = prefab.transform.position;
        currentY = basePosition.y;

        float meshHeight = 1f;

        // Try to get prefab mesh height using its Renderer bounds
        Renderer rend = prefab.GetComponent<Renderer>();
        if (rend == null)
            rend = prefab.GetComponentInChildren<Renderer>();

        if (rend != null)
        {
            meshHeight = rend.bounds.size.y;
        }
        else
        {
            Debug.LogWarning("Prefab has no Renderer. Using default mesh height of 1.");
        }

        int count = 0;
        while (count < maxInstances && currentY <= basePosition.y + currentHeightLimit)
        {
            Vector3 spawnPos = new Vector3(basePosition.x, currentY, basePosition.z);
            Instantiate(prefab, spawnPos, prefab.transform.rotation);

            float randomSpacing = Random.Range(minSpacing, maxSpacing);
            currentY += meshHeight + randomSpacing;

            count++;
        }
    }
} 