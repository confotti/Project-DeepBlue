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
        if (pm == null || !pm.IsSwimming)
            return;

        Rigidbody rb = pm.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("PlayerMovement requires a Rigidbody!");
            return;
        }

        Vector3 targetPosition = transform.position + teleportToLocation;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        RigidbodyInterpolation previousInterpolation = rb.interpolation;
        rb.interpolation = RigidbodyInterpolation.None;

        rb.position = targetPosition;

        Physics.SyncTransforms();

        rb.interpolation = previousInterpolation;

        pm.StateMachine.ChangeState(pm.StandingState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + teleportToLocation, 0.5f);
    }
} 