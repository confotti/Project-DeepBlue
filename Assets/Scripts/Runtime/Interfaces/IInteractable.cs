using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    public string InteractText{ get; }
    public void Interact(PlayerInteract interactor);
    public void EndInteraction();

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
}
