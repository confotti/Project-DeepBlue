using UnityEngine;
using UnityEngine.Events;

public class PickUp : MonoBehaviour, IInteractable
{
    public GameObject holder;
    public Transform pos; 
    public GameObject[] loot;

    public string InteractText => throw new System.NotImplementedException();

    public UnityAction<IInteractable> OnInteractionComplete { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void EndInteraction()
    {
        throw new System.NotImplementedException();
    }

    public void Interact(PlayerInteract interactor)
    {
        int randomIndex = Random.Range(0, loot.Length);
        Instantiate(loot[randomIndex], pos.position, Quaternion.identity);

        holder.SetActive(false); 
    }
}
