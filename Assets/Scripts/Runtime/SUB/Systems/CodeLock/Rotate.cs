using System.Collections;
using UnityEngine;
using System;

public class Rotate : MonoBehaviour
{
    public static event Action<string, int> Rotated = delegate { };

    private bool coroutineAllowed;
    private int numberShown;

    [Header("Rotation")]
    [SerializeField] private float rotationAmount = 40f;
    [SerializeField] private int rotationSteps = 12;
    [SerializeField] private float rotationDelay = 0.01f;

    private void Start()
    {
        coroutineAllowed = true;
        numberShown = 1;
    }

    private void OnMouseDown()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(RotateWheel());
        }
    }

    private IEnumerator RotateWheel()
    {
        coroutineAllowed = false;

        float rotationPerStep = rotationAmount / rotationSteps;

        for (int i = 0; i < rotationSteps; i++)
        {
            transform.Rotate(rotationPerStep, 0f, 0f);
            yield return new WaitForSeconds(rotationDelay);
        }

        coroutineAllowed = true;

        numberShown += 1;

        if (numberShown > 9)
        {
            numberShown = 0;
        }

        Rotated(name, numberShown);
    }
} 