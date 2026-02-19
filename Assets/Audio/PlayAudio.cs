using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SwimmingAudioController : MonoBehaviour
{
    private AudioSource _audioSource;
    private PlayerMovement _player;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _player = PlayerMovement.Instance;

        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
    }

    private void Update()
    {
        if (_player == null) return;

        if (_player.IsSwimming)
        {
            if (!_audioSource.isPlaying)
                _audioSource.Play();
        }
        else
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
        }
    }
}