using UnityEngine;
using UnityEngine.Events;

public class ChooseNextBiomeButton : MonoBehaviour, IInteractable
{
    [SerializeField] private BiomePort _biomePort;
    [SerializeField] private int _nextBiomeIndex;

    public string InteractText => "";

    UnityAction<IInteractable> IInteractable.OnInteractionComplete { get; set; }

    public void EndInteraction()
    {
        
    }

    public void Interact(PlayerInteract interactor)
    {
        _biomePort.OnChooseNextBiome?.Invoke(_nextBiomeIndex);
    }
}
