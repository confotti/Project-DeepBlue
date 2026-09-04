using System.Collections;
using UnityEngine;

public class LockControl : MonoBehaviour
{
    private int[] result;
    private int[] correctCombination;

    [SerializeField] private Rotate[] wheels;
    [SerializeField] private GameObject[] wheelHighlights;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform lockCameraPosition;
    [SerializeField] private float cameraTransitionTime = 0.2f;

    [Header("Camera Control")]
    [SerializeField] private MonoBehaviour[] playerMovement;

    public bool IsSolved => combinationSolved;

    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;

    private bool lockActive;
    private bool cameraMoving;
    private bool combinationSolved;

    private int selectedWheel;

    private void Start()
    {
        correctCombination = new int[] { 3, 7, 9, 1 };
        result = new int[wheels.Length];

        for (int i = 0; i < wheels.Length; i++)
        {
            result[i] = wheels[i].CurrentNumber;
        }

        Rotate.Rotated += CheckResults;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        DisableAllHighlights();
    }

    private void Update()
    {
        if (!lockActive || cameraMoving || combinationSolved)
        {
            return;
        }

        HandleWheelSelection();
        HandleWheelRotation();

        if (Input.GetMouseButtonDown(1))
        {
            ExitLock();
        }
    }

    private void OnMouseDown()
    {
        if (!lockActive && !combinationSolved)
        {
            EnterLock();
        }
    }

    private void EnterLock()
    {
        lockActive = true;
        cameraMoving = true;

        selectedWheel = 0;
        UpdateWheelHighlight();

        originalCameraLocalPosition = cameraTransform.localPosition;
        originalCameraLocalRotation = cameraTransform.localRotation;

        SetPlayerScripts(false);

        StartCoroutine(MoveCameraToLock());
    }

    private void SetPlayerScripts(bool enabled)
    {
        if (playerMovement == null)
        {
            return;
        }

        foreach (MonoBehaviour script in playerMovement)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }

    private void HandleWheelSelection()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            SelectNextWheel();
        }

        if (Input.GetKeyDown(KeyCode.A)) 
        {
            SelectPreviousWheel();
        }
    }

    private void SelectNextWheel()
    {
        selectedWheel++;

        if (selectedWheel >= wheels.Length)
        {
            selectedWheel = 0;
        }

        UpdateWheelHighlight();
    }

    private void SelectPreviousWheel()
    {
        selectedWheel--;

        if (selectedWheel < 0)
        {
            selectedWheel = wheels.Length - 1;
        }

        UpdateWheelHighlight();
    }

    private void HandleWheelRotation()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            wheels[selectedWheel].RotateUp();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            wheels[selectedWheel].RotateDown();
        }
    }

    private void UpdateWheelHighlight()
    {
        if (wheelHighlights == null || wheelHighlights.Length == 0)
        {
            return;
        }

        for (int i = 0; i < wheelHighlights.Length; i++)
        {
            if (wheelHighlights[i] != null)
            {
                wheelHighlights[i].SetActive(i == selectedWheel);
            }
        }
    }

    private void DisableAllHighlights()
    {
        if (wheelHighlights == null)
        {
            return;
        }

        foreach (GameObject highlight in wheelHighlights)
        {
            if (highlight != null)
            {
                highlight.SetActive(false);
            }
        }
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
            t = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(startPosition,lockCameraPosition.position,t);
            cameraTransform.rotation = Quaternion.Slerp(startRotation,lockCameraPosition.rotation,t);

            yield return null;
        }

        cameraTransform.position = lockCameraPosition.position;
        cameraTransform.rotation = lockCameraPosition.rotation;

        cameraMoving = false;
    }

    private void ExitLock()
    {
        if (!lockActive || cameraMoving)
        {
            return;
        }

        cameraMoving = true;
        DisableAllHighlights();

        StartCoroutine(MoveCameraBack(false));
    }

    private IEnumerator MoveCameraBack(bool solved)
    {
        Vector3 startPosition = cameraTransform.position;
        Quaternion startRotation = cameraTransform.rotation;

        Vector3 targetPosition = cameraTransform.parent.TransformPoint(originalCameraLocalPosition);
        Quaternion targetRotation =cameraTransform.parent.rotation *originalCameraLocalRotation;

        float elapsedTime = 0f;

        while (elapsedTime < cameraTransitionTime)
        {
            elapsedTime += Time.deltaTime;

            float t = elapsedTime / cameraTransitionTime;
            t = Mathf.SmoothStep(0f, 1f, t);

            cameraTransform.position = Vector3.Lerp(startPosition,targetPosition,t);
            cameraTransform.rotation = Quaternion.Slerp(startRotation,targetRotation,t);

            yield return null;
        }

        cameraTransform.localPosition = originalCameraLocalPosition;
        cameraTransform.localRotation = originalCameraLocalRotation;

        lockActive = false;
        cameraMoving = false;

        SetPlayerScripts(true);

        if (solved)
        {
            combinationSolved = true;

            gameObject.SetActive(false);
        }
    }

    private void CheckResults(Rotate wheel, int number)
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] == wheel)
            {
                result[i] = number;
                break;
            }
        }

        if (result[0] == correctCombination[0] &&
            result[1] == correctCombination[1] &&
            result[2] == correctCombination[2] &&
            result[3] == correctCombination[3])
        {
            combinationSolved = true;
            DisableAllHighlights();
            cameraMoving = true;
            StartCoroutine(MoveCameraBack(true)); 
        }
    }

    private void OnDestroy()
    {
        Rotate.Rotated -= CheckResults;
    }
}