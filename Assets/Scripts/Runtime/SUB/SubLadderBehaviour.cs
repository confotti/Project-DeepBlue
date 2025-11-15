using UnityEngine;
using UnityEngine.Events;

public class SubLadderBehaviour : MonoBehaviour, IInteractable
{
    [SerializeField] private Vector3 teleportToLocation;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void EndInteraction()
    {
        
    }

    public void Interact(PlayerInteract interactor, out bool interactSuccessful)
    {
        interactor.transform.position = transform.position + teleportToLocation;
        interactor.GetComponent<PlayerMovement>().currentState = PlayerMovement.States.standing;
        interactSuccessful = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + teleportToLocation, 0.5f);
    }
}
