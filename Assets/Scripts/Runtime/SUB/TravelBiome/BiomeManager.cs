using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BiomeManager : MonoBehaviour
{
    public static Action CommandStartLoadingNextBiome;
    public static Action<int> OnChooseNextBiome;
    public static Action OnFinishLoadingBiome;
    public static Action OnReadyToUnload;

    public static BiomeSubSplineHolder BiomeSubSplineHolder;

    public List<BiomeReference> Biomes;

    private int currentBiomeIndex = -1;
    public int nextBiomeIndex = -1;
    private bool isLoadingNextBiome = false;

    private bool readyToUnloadPrevious = false;

    void OnEnable()
    {
        CommandStartLoadingNextBiome += LoadNextBiome;
        OnChooseNextBiome += ChooseNextBiome;
        OnReadyToUnload += ReadyToUnloadLastBiome;
    }

    void OnDisable()
    {
        CommandStartLoadingNextBiome -= LoadNextBiome;
        OnChooseNextBiome -= ChooseNextBiome;
        OnReadyToUnload -= ReadyToUnloadLastBiome;
    }

    void Awake()
    {
        Debug.Log(nextBiomeIndex);
        LoadBiome(nextBiomeIndex);
    }

    private void LoadBiome(int biomeIndex)
    {
        if (biomeIndex == currentBiomeIndex)
        {
            Debug.LogWarning("Cannot load the same biome that is currently loaded.");
            return;
        }
        else if (biomeIndex < 0 || biomeIndex > Biomes.Count - 1)
        {
            Debug.LogWarning("Biome to be loaded cannot be null");
            return;
        }

        // Start loading the biome asynchronously
        isLoadingNextBiome = true;

        // Load the new biome asynchronously
        StartCoroutine(LoadBiomeAsync(biomeIndex));
    }

    private void LoadNextBiome()
    {
        LoadBiome(nextBiomeIndex);
    }

    private IEnumerator LoadBiomeAsync(int biomeIndex)
    {
        BiomeReference biome = Biomes[biomeIndex];

        // Start loading the new biome scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(biome.SceneName, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OnFinishLoadingBiome?.Invoke();
        isLoadingNextBiome = false;

        

        readyToUnloadPrevious = false;

        // After loading, unload the current biome
        if (currentBiomeIndex > -1 && currentBiomeIndex < Biomes.Count)
        {
            while (!readyToUnloadPrevious)
            {
                yield return null;
            }

            UnloadBiome(Biomes[currentBiomeIndex]);
        }

        // Set the new biome as the current biome
        currentBiomeIndex = biomeIndex;
    }

    private void UnloadBiome(BiomeReference biome)
    {
        // Unload the scene for the current biome
        SceneManager.UnloadSceneAsync(biome.SceneName);
    }

    private void ChooseNextBiome(int index)
    {
        if (index < 0 || index > Biomes.Count - 1)
        {
            Debug.LogWarning("Index is outside of Biomes count");
            return;
        }

        if (index == currentBiomeIndex)
        {
            Debug.LogWarning("Cannot choose the same biome that is currently loaded");
            return;
        }

        nextBiomeIndex = index;
    }

    private void ReadyToUnloadLastBiome()
    {
        readyToUnloadPrevious = true;
    }
}



[System.Serializable]
public class BiomeReference
{
    [SerializeField] private string _sceneName;
    public string SceneName => _sceneName;
}
