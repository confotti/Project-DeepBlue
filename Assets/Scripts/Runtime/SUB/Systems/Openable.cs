using System.Collections;
using UnityEngine;

public class Openable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactText = "Open";

    public string InteractText => interactText;

    [SerializeField] private LockControl requiredLock;

    [Header("Object To Animate")]
    [SerializeField] private Transform movingPart;

    [Header("Open Position")]
    [SerializeField] private Vector3 openRotation;
    [SerializeField] private Vector3 openPosition;

    [Header("Animation")]
    [SerializeField] private float openDuration = 0.5f;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private Vector3 closedPosition;
    private Vector3 openedPosition;

    private bool isOpen;
    private bool isAnimating;

    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Awake()
    {
        closedRotation = movingPart.localRotation;
        closedPosition = movingPart.localPosition;

        openedRotation = Quaternion.Euler(openRotation);
        openedPosition = openPosition;
    }

    public void Interact(PlayerInteract interactor)
    {
        if (isAnimating)
            return;

        if (requiredLock != null && !requiredLock.IsSolved)
        {
            Debug.Log("This container is locked.");
            return;
        }

        if (!isOpen)
        {
            StartCoroutine(AnimateOpen());
        }
        else
        {
            StartCoroutine(AnimateClose());
        }
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;

        Quaternion startRotation = movingPart.localRotation;
        Vector3 startPosition = movingPart.localPosition;

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / openDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            movingPart.localRotation = Quaternion.Slerp(startRotation,openedRotation,t);
            movingPart.localPosition = Vector3.Lerp(startPosition,openedPosition,t);

            yield return null;
        }

        movingPart.localRotation = openedRotation;
        movingPart.localPosition = openedPosition;

        isOpen = true;
        isAnimating = false;
    }

    private IEnumerator AnimateClose()
    {
        isAnimating = true;

        Quaternion startRotation = movingPart.localRotation;
        Vector3 startPosition = movingPart.localPosition;

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / openDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            movingPart.localRotation = Quaternion.Slerp(startRotation,closedRotation,t);
            movingPart.localPosition = Vector3.Lerp(startPosition,closedPosition,t);

            yield return null;
        }

        movingPart.localRotation = closedRotation;
        movingPart.localPosition = closedPosition;

        isOpen = false;
        isAnimating = false;
    }

    public void EndInteraction()
    {

    }
}