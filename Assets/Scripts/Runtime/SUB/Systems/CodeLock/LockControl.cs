using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class LockControl : MonoBehaviour
{
    private int[] result;
    private int[] correctCombination;

    [SerializeField] private Rotate[] wheels;
    [SerializeField] private GameObject[] wheelHighlights;

    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineCamera mainCamera;
    [SerializeField] private CinemachineCamera lockCamera;

    [Header("Camera Control")]
    [SerializeField] private MonoBehaviour[] playerMovement;

    public bool IsSolved => combinationSolved;

    private bool lockActive;
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

        mainCamera.Priority = 10;
        lockCamera.Priority = 0;

        DisableAllHighlights();
    }

    private void Update()
    {
        if (!lockActive || combinationSolved)
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

        selectedWheel = 0;
        UpdateWheelHighlight();

        mainCamera.Priority = 0;
        lockCamera.Priority = 10;

        SetPlayerScripts(false);
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

    private void ExitLock()
    {
        if (!lockActive)
        {
            return;
        }

        lockActive = false;

        DisableAllHighlights();

        lockCamera.Priority = 0;
        mainCamera.Priority = 10;

        SetPlayerScripts(true);
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

            lockCamera.Priority = 0;
            mainCamera.Priority = 10;

            SetPlayerScripts(true);
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        Rotate.Rotated -= CheckResults;
    }
} 