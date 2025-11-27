using UnityEngine;
using UnityEngine.Events;

public class InteractionButton : MonoBehaviour, IInteractable
{
    public SubmarineController submarine;

    public string InteractText => throw new System.NotImplementedException();

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteract interactor)
    {
        submarine.ToggleSub();
    }

    public void EndInteraction()
    {
        
    }
}