using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BiomePort", menuName = "Scriptable Objects/BiomePort")]
public class BiomePort : ScriptableObject
{
    public Action CommandStartLoadingNextBiome;
    public Action<int> OnChooseNextBiome;
    public Action OnFinishLoadingBiome;
    public Action OnReadyToUnload;

    public List<BiomeReference> Biomes;
}



[System.Serializable]
public class BiomeReference
{
    [SerializeField] private string _sceneName;
    public string SceneName => _sceneName;
}