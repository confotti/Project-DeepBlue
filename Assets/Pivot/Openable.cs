using System.Collections;
using UnityEngine;

public class Openable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactText = "Open";

    public string InteractText => interactText;

    [Header("Object To Animate")]
    [SerializeField] private Transform movingPart;

    [Header("Open Position")]
    [SerializeField] private Vector3 openRotation;

    [Header("Animation")]
    [SerializeField] private float openDuration = 0.5f;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private bool isOpen;
    private bool isAnimating;

    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Awake()
    {
        closedRotation = movingPart.localRotation;
        openedRotation = Quaternion.Euler(openRotation);
    }

    public void Interact(PlayerInteract interactor)
    {
        if (isAnimating)
            return;

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
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / openDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            movingPart.localRotation = Quaternion.Slerp(startRotation,openedRotation,t);

            yield return null;
        }

        movingPart.localRotation = openedRotation;

        isOpen = true;
        isAnimating = false;
    }

    private IEnumerator AnimateClose()
    {
        isAnimating = true;

        Quaternion startRotation = movingPart.localRotation;
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / openDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            movingPart.localRotation = Quaternion.Slerp(startRotation,closedRotation,t);

            yield return null;
        }

        movingPart.localRotation = closedRotation;

        isOpen = false;
        isAnimating = false;
    }

    public void EndInteraction()
    {

    }
}