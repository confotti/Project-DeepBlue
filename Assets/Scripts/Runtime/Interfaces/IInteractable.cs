using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    public void Interact(PlayerInteract interactor, out bool interactSuccessful);
    public void EndInteraction();

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
}
