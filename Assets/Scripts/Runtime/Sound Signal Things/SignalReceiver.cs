using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class SignalReceiver : MonoBehaviour
{
    [SerializeField] private SignalPort signalPort;

    [Header("Detection")]
    [SerializeField] private float maxDetectionAngle = 20f;

    [SerializeField, Range(0.0f, 1.0f), Description("The percantage from max detection angle where the signal will be at it's strongest. " +
        "For example if at 0.9 it will be a full strength signal when you are within the closest 10% of max detection angle. At 0 it will always be max strength signal")] 
    private float fullStrengthCutoff = 0.9f;

    private float minDot;

    private List<float> signalStrengths = new();

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

        signalPort.distance?.Invoke(GetCurrentDistance());
        signalPort.audioSource?.Invoke(CurrentSignal.signalAudio);
        signalPort.signalStrengths?.Invoke(signalStrengths.ToArray());


        if (CurrentSignal != null)
        {
            Debug.Log("There is a signal called " + CurrentSignal.signalName + " " + (int)(GetCurrentDistance()) + " meters away");
        }
    }

    private void FindBestSignal()
    {
        signalStrengths.Clear();
        CurrentSignal = null;

        float bestStrength = 0;

        foreach (SignalEmitter signal in SignalManager.Signals)
        {
            if (signal == null)
                continue;

            Vector3 toSignal = signal.transform.position - transform.position;
            float distance = toSignal.magnitude;

            Vector3 directionToSignal = toSignal.normalized;

            float dot = Vector3.Dot(transform.forward, directionToSignal);

            if (dot < minDot) continue;

            float signalStrength = Mathf.InverseLerp(minDot, Mathf.Lerp(minDot, 1, fullStrengthCutoff), dot);
            signalStrengths.Add(signalStrength);

            if (signalStrength > bestStrength)
            {
                bestStrength = signalStrength;
                CurrentSignal = signal;
            }
        }

    }

    public float GetCurrentDistance()
    {
        if (CurrentSignal == null)
            return -1;

        return Vector3.Distance(
            transform.position,
            CurrentSignal.transform.position
        ) / 10;
    }
}
