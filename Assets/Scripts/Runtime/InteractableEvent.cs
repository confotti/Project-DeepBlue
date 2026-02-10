using UnityEngine;
using UnityEngine.Events;

public class InteractableEvent : MonoBehaviour, IInteractable
{
    public UnityEvent interactableEvent;

    public string InteractText => throw new System.NotImplementedException();

    public UnityAction<IInteractable> OnInteractionComplete { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void EndInteraction()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerInteract interactor)
    {
        interactableEvent?.Invoke();
    }
}
