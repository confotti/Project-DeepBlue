using UnityEngine;

public class SignalReceiver : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float maxDetectionAngle = 20f;

    private float minDot;

    public SignalEmitter CurrentSignal { get; private set; }

    private void OnValidate()
    {
        minDot = Mathf.Cos(maxDetectionAngle * Mathf.Deg2Rad);
    }

    private void Start()
    {
        minDot = Mathf.Cos(maxDetectionAngle * Mathf.Deg2Rad);
    }

    private void Update()
    {
        FindBestSignal();
    }

    private void FindBestSignal()
    {
        CurrentSignal = null;

        float bestScore = 0;

        foreach (SignalEmitter signal in SignalManager.Signals)
        {
            if (signal == null)
                continue;

            Vector3 toSignal = signal.transform.position - transform.position;
            float distance = toSignal.magnitude;

            Vector3 directionToSignal = toSignal.normalized;

            float dot = Vector3.Dot(
                transform.forward,
                directionToSignal
            );

            float signalStrength = Mathf.InverseLerp(minDot, 1f, dot);

            if (signalStrength > bestScore)
            {
                bestScore = signalStrength;
                CurrentSignal = signal;
            }
        }

        if(CurrentSignal != null)
        {
            Debug.Log("There is a signal called " + CurrentSignal.signalName);
        }
    }

    public float GetCurrentDistance()
    {
        if (CurrentSignal == null)
            return -1;

        return Vector3.Distance(
            transform.position,
            CurrentSignal.transform.position
        );
    }
}
