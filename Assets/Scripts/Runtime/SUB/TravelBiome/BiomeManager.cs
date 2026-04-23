using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BiomeManager : MonoBehaviour
{
    public static BiomeSubSplineHolder BiomeSubSplineHolder;

    [SerializeField] private BiomePort _biomePort;

    private int currentBiomeIndex = -1;
    [SerializeField] private int _nextBiomeIndex = -1;
    private bool isLoadingNextBiome = false;

    private bool readyToUnloadPrevious = false;

    void OnEnable()
    {
        _biomePort.CommandStartLoadingNextBiome += LoadNextBiome;
        _biomePort.OnChooseNextBiome += ChooseNextBiome;
        _biomePort.OnReadyToUnload += ReadyToUnloadLastBiome;
    }

    void OnDisable()
    {
        _biomePort.CommandStartLoadingNextBiome -= LoadNextBiome;
        _biomePort.OnChooseNextBiome -= ChooseNextBiome;
        _biomePort.OnReadyToUnload -= ReadyToUnloadLastBiome;
    }

    void Awake()
    {
        LoadBiome(_nextBiomeIndex);
    }

    private void LoadBiome(int biomeIndex)
    {
        if (biomeIndex == currentBiomeIndex)
        {
            Debug.LogWarning("Cannot load the same biome that is currently loaded.");
            return;
        }
        else if (biomeIndex < 0 || biomeIndex > _biomePort.Biomes.Count - 1)
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
        LoadBiome(_nextBiomeIndex);
    }

    private IEnumerator LoadBiomeAsync(int biomeIndex)
    {
        BiomeReference biome = _biomePort.Biomes[biomeIndex];

        // Start loading the new biome scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(biome.SceneName, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        _biomePort.OnFinishLoadingBiome?.Invoke();
        isLoadingNextBiome = false;

        

        readyToUnloadPrevious = false;

        // After loading, unload the current biome
        if (currentBiomeIndex > -1 && currentBiomeIndex < _biomePort.Biomes.Count)
        {
            while (!readyToUnloadPrevious)
            {
                yield return null;
            }

            UnloadBiome(_biomePort.Biomes[currentBiomeIndex]);
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
        if (index < 0 || index > _biomePort.Biomes.Count - 1)
        {
            Debug.LogWarning("Index is outside of Biomes count");
            return;
        }

        if (index == currentBiomeIndex)
        {
            Debug.LogWarning("Cannot choose the same biome that is currently loaded");
            return;
        }

        _nextBiomeIndex = index;
    }

    private void ReadyToUnloadLastBiome()
    {
        readyToUnloadPrevious = true;
    }
}

