using System.Collections;
using UnityEngine;
using System;

public class Rotate : MonoBehaviour
{
    public static event Action<Rotate, int> Rotated = delegate { };

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

    public void RotateUp()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(RotateWheel(1));
        }
    }

    public void RotateDown()
    {
        if (coroutineAllowed)
        {
            StartCoroutine(RotateWheel(-1));
        }
    }

    private IEnumerator RotateWheel(int direction)
    {
        coroutineAllowed = false;

        float rotationPerStep = rotationAmount / rotationSteps;

        for (int i = 0; i < rotationSteps; i++)
        {
            transform.Rotate(
                rotationPerStep * direction,
                0f,
                0f
            );

            yield return new WaitForSeconds(rotationDelay);
        }

        numberShown += direction;

        // Numbers are 1-9 only
        if (numberShown > 9)
        {
            numberShown = 1;
        }
        else if (numberShown < 1)
        {
            numberShown = 9;
        }

        coroutineAllowed = true;

        // Tell LockControl which wheel moved
        Rotated(this, numberShown);
    }
}