using UnityEngine.Events; 
using UnityEngine;
using System.Collections.Generic;

public class LightBehaviour : MonoBehaviour, IInteractable
{
    public List<GameObject> lightSources = new List<GameObject>();

    private bool lightOn = true; 

    public string InteractText => throw new System.NotImplementedException();

    public UnityAction<IInteractable> OnInteractionComplete { get; set; } 

    public void Interact(PlayerInteract interactor)
    {
        lightOn = !lightOn;

        foreach (var obj in lightSources)
            obj.SetActive(lightOn);

        
    } 

    public void EndInteraction()
    {
        
    }
}
