using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnableObjectOnTrigger : MonoBehaviour
{
    [SerializeField] private List<GameObject> _objectsToEnable = new ();
    [SerializeField] private float _duration = 5f;

    private bool _hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (_hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            _hasTriggered = true;
            StartCoroutine(EnableTemporary());
        }
    }

    private IEnumerator EnableTemporary()
    {
        foreach (GameObject obj in _objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        yield return new WaitForSeconds(_duration);

        foreach (GameObject obj in _objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
} 