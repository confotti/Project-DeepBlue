using UnityEngine;
using UnityEngine.Events;

public class SubLadderBehaviour : MonoBehaviour, IInteractable
{

    [SerializeField] private string interactText = "";
    public string InteractText => interactText;

    [SerializeField] private Vector3 teleportToLocation;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void EndInteraction()
    {
        
    }

    public void Interact(PlayerInteract interactor)
    {
        PlayerMovement pm = interactor.GetComponent<PlayerMovement>();
        if (!pm.IsSwimming) return;

        interactor.transform.position = transform.position + teleportToLocation;
        pm.StateMachine.ChangeState(pm.StandingState);
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
