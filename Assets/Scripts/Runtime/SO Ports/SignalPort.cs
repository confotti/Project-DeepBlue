using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SignalPort", menuName = "Ports/Signal Port")]
public class SignalPort : ScriptableObject
{
    public Action<float> distance;
    public Action<AudioSource> audioSource;
    public Action<float[]> signalStrengths;
}
