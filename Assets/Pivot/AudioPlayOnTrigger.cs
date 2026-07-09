using UnityEngine;

public class AudioPlayOnTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private GameObject triggerObject;
    [SerializeField] private string _playerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_playerTag)) return;
        if (audioSource != null) audioSource.Play();
        if (triggerObject != null) triggerObject.SetActive(false);
    }
} 