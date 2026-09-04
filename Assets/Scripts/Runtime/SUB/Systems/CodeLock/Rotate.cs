using System.Collections;
using UnityEngine;
using System;

public class Rotate : MonoBehaviour
{
    public static event Action<Rotate, int> Rotated = delegate { };

    private bool coroutineAllowed;
    private int numberShown;
    public int CurrentNumber => numberShown; 

    [Header("Rotation")]
    [SerializeField] private float rotationAmount = 40f;
    [SerializeField] private int rotationSteps = 12;
    [SerializeField] private float rotationDelay = 0.01f;

    private void Start()
    {
        coroutineAllowed = true;

        numberShown = UnityEngine.Random.Range(1, 10);
        int stepsFromOne = numberShown - 1;

        transform.Rotate(stepsFromOne * rotationAmount,0f,0f);
        Rotated(this, numberShown);
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
            transform.Rotate(rotationPerStep * direction,0f,0f);
            yield return new WaitForSeconds(rotationDelay);
        }

        numberShown += direction;

        if (numberShown > 9)
        {
            numberShown = 1;
        }
        else if (numberShown < 1)
        {
            numberShown = 9;
        }

        coroutineAllowed = true;
        Rotated(this, numberShown);
    }
}