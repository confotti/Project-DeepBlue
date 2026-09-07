using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SignalReceiver : MonoBehaviour
{
    [SerializeField] private SignalPort signalPort;

    [Header("Detection")]
    [SerializeField] private float maxDetectionAngle = 20f;

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

            float signalStrength = Mathf.InverseLerp(minDot, 1f, dot);
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
