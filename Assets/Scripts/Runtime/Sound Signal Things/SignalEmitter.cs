using UnityEngine;

public class SignalEmitter : MonoBehaviour
{
    public string signalName;
    public AudioSource signalAudio;

    private void OnEnable()
    {
        SignalManager.Register(this);
    }

    private void OnDisable()
    {
        SignalManager.Unregister(this);
    }
}