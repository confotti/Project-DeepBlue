using System.Collections;
using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    public void DisableAfter(float delay)
    {
        StopAllCoroutines();
        StartCoroutine(Disable(delay));
    }

    private IEnumerator Disable(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
} 