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

    public static BiomeSubSplineHolder BiomeSubSplineHolder;

    public List<BiomeReference> Biomes;

    private BiomeReference currentBiome = null;
    private BiomeReference nextBiome = null;
    private bool isLoadingNextBiome = false;

    private bool readyToUnloadPrevious = false;

    void OnEnable()
    {
        CommandStartLoadingNextBiome += LoadNextBiome;
        OnChooseNextBiome += ChooseNextBiome;
    }

    void OnDisable()
    {
        CommandStartLoadingNextBiome -= LoadNextBiome;
        OnChooseNextBiome -= ChooseNextBiome;
    }

    private void LoadBiome(BiomeReference biome)
    {
        if (biome == currentBiome)
        {
            Debug.LogWarning("Cannot load the same biome that is currently loaded.");
            return;
        }
        else if (biome == null)
        {
            Debug.LogWarning("Biome to be loaded cannot be null");
            return;
        }

        // Start loading the biome asynchronously
        isLoadingNextBiome = true;

        // Load the new biome asynchronously
        StartCoroutine(LoadBiomeAsync(biome));
    }

    private void LoadNextBiome()
    {
        LoadBiome(nextBiome);
    }

    private IEnumerator LoadBiomeAsync(BiomeReference biome)
    {
        // Start loading the new biome scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(biome.SceneName, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        OnFinishLoadingBiome?.Invoke();

        while (!readyToUnloadPrevious)
        {
            yield return null;
        }

        // After loading, unload the current biome
        if (currentBiome != null)
        {
            UnloadBiome(currentBiome);
        }

        // Set the new biome as the current biome
        currentBiome = biome;
        isLoadingNextBiome = false;

        readyToUnloadPrevious = false;
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

        if (Biomes[index] == currentBiome)
        {
            Debug.LogWarning("Cannot choose the same biome that is currently loaded");
            return;
        }

        nextBiome = Biomes[index];
    }
}




[System.Serializable]
public class BiomeReference
{
    [SerializeField] private string _sceneName;
    public string SceneName => _sceneName;
}
