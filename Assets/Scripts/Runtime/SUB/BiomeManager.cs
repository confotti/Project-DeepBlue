using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BiomeManager : MonoBehaviour
{
    [Serializable]
    public class Biome
    {
        public string sceneName;
    }

    public List<Biome> Biomes;

    private Biome currentBiome = null;
    private Biome nextBiome = null;
    private bool isLoadingNextBiome = false;

    private bool readyToUnloadPrevious = false;

    void Update()
    {

        if (!isLoadingNextBiome)
        {
            if (nextBiome != null && nextBiome != currentBiome)
            {
                LoadBiome(nextBiome);
            }
        }
    }


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
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(biome.sceneName, LoadSceneMode.Additive);

        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone || !readyToUnloadPrevious)
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
        SceneManager.UnloadSceneAsync(biome.sceneName);
    }
}

