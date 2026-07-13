using UnityEngine;
using System.Collections;

public class EnableObjectOnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject _objectToEnable;
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
        _objectToEnable.SetActive(true);

        yield return new WaitForSeconds(_duration);

        _objectToEnable.SetActive(false);
    }
} 