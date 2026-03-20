using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BiomeManager : MonoBehaviour
{
    [Serializable]
    public class Biome
    {
        public SceneAsset scene;
    }

    public static Action CommandStartLoading;
    public static Action OnFinishLoadingBiome;

    public List<Biome> Biomes;

    private Biome currentBiome = null;
    private Biome nextBiome = null;
    private bool isLoadingNextBiome = false;

    private bool readyToUnloadPrevious = false;

    private void LoadBiome(Biome biome)
    {
        // Start loading the biome asynchronously
        isLoadingNextBiome = true;

        // Load the new biome asynchronously
        StartCoroutine(LoadBiomeAsync(biome));
    }

    private IEnumerator LoadBiomeAsync(Biome biome)
    {
        // Start loading the new biome scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(biome.scene.name, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!readyToUnloadPrevious)
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

    private void UnloadBiome(Biome biome)
    {
        // Unload the scene for the current biome
        SceneManager.UnloadSceneAsync(biome.scene.name);
    }
}
