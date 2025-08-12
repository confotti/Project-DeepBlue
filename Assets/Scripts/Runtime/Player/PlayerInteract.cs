using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 2f;
    [SerializeField] private LayerMask interactLayer;

    //References
    private PlayerInputHandler inputHandler;

    void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        inputHandler.OnInteract += Interact;
    }

    private void OnDisable()
    {
        inputHandler.OnInteract -= Interact;
    }

    void Update()
    {
        
    }

    private void Interact()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, interactLayer))
        {
            var interactable = hit.transform.GetComponent<IInteractable>();
            interactable.Interact();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * range);
    }
}
