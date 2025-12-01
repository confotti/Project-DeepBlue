using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 2f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private GameObject head;

    public bool IsInteracting { get; private set; }

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
        var hits = Physics.RaycastAll(head.transform.position, head.transform.forward, range, interactLayer);
        interactable = null;
        foreach (var item in hits)
        {
            if (item.transform.TryGetComponent<IInteractable>(out interactable)) break;
        }

        UpdateUI();
    }

    private void Interact()
    {
        if (interactable != null)
        {
            interactable.Interact(this);
            //IsInteracting = true;
        }
    }

    //Fix IsInteracting if necessary
    private void EndInteraction()
    {
        IsInteracting = false;
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
        Gizmos.DrawLine(head.transform.position, head.transform.position + head.transform.forward * range);
    }
}
