using System.Collections;
using UnityEngine;

public class LockControl : MonoBehaviour
{
    private int[] result;
    private int[] correctCombination;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform lockCameraPosition;
    [SerializeField] private float cameraTransitionTime = 0.2f;

    [Header("Camera Control")]
    [SerializeField] private MonoBehaviour playerMovement;

    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;

    private bool lockActive;
    private bool cameraMoving;

    private void Start()
    {
        result = new int[] { 5, 5, 5, 5 };
        correctCombination = new int[] { 3, 7, 9, 1 };

        Rotate.Rotated += CheckResults;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (lockActive && !cameraMoving && Input.GetMouseButtonDown(1))
        {
            ExitLock();
        }
    }

    private void OnMouseDown()
    {
        if (!lockActive)
        {
            EnterLock();
        }
    }

    private void EnterLock()
    {
        lockActive = true;
        cameraMoving = true;

        originalCameraLocalPosition = cameraTransform.localPosition;
        originalCameraLocalRotation = cameraTransform.localRotation;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        StartCoroutine(MoveCameraToLock());
    }

    private IEnumerator MoveCameraToLock()
    {
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < cameraTransitionTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / cameraTransitionTime;

            // Smooth transition
            t = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(
                startPosition,
                lockCameraPosition.position,
                t
            );

            cameraTransform.rotation = Quaternion.Slerp(
                startRotation,
                lockCameraPosition.rotation,
                t
            );

            yield return null;
        }

        cameraTransform.position = lockCameraPosition.position;
        cameraTransform.rotation = lockCameraPosition.rotation;

        cameraMoving = false;
    }

    private void ExitLock()
    {
        cameraMoving = true;

        StartCoroutine(MoveCameraBack());
    }

    private IEnumerator MoveCameraBack()
    {
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        Vector3 targetPosition = cameraTransform.parent.TransformPoint(
            originalCameraLocalPosition
        );

        Quaternion targetRotation = cameraTransform.parent.rotation *
                                    originalCameraLocalRotation;

        float elapsedTime = 0f;

        while (elapsedTime < cameraTransitionTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / cameraTransitionTime;

            // Smooth transition
            t = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                t
            );

            cameraTransform.rotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            yield return null;
        }

        cameraTransform.localPosition = originalCameraLocalPosition;
        cameraTransform.localRotation = originalCameraLocalRotation;

        lockActive = false;
        cameraMoving = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    private void CheckResults(string wheelName, int number)
    {
        switch (wheelName)
        {
            case "wheel1":
                result[0] = number;
                break;

            case "wheel2":
                result[1] = number;
                break;

            case "wheel3":
                result[2] = number;
                break;

            case "wheel4":
                result[3] = number;
                break;
        }

        if (result[0] == correctCombination[0] &&
            result[1] == correctCombination[1] &&
            result[2] == correctCombination[2] &&
            result[3] == correctCombination[3])
        {
            Debug.Log("Opened");
        }
    }

    private void OnDestroy()
    {
        Rotate.Rotated -= CheckResults;
    }
} 