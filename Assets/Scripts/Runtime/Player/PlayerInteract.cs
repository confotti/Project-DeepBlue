using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 2f;
    [SerializeField] private LayerMask interactLayer;

    //References
    private PlayerInputHandler inputHandler;
    private IInteractable interactable;

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
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, range, interactLayer))
        {
            interactable = hit.transform.GetComponent<IInteractable>();
        }
        else
        {
            interactable = null;
        }

        UpdateUI();
    }

    private void Interact()
    {
        if(interactable != null) interactable.Interact(gameObject);
    }

    private void UpdateUI()
    {
        if (interactable == null)
        {
            //Hide UI
        }
        else
        {
            //Show UI
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * range);
    }
}
