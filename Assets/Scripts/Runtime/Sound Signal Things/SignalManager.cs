using System.Collections.Generic;

public static class SignalManager
{
    public static List<SignalEmitter> Signals { get; } = new();

    public static void Register(SignalEmitter signal)
    {
        if (!Signals.Contains(signal))
            Signals.Add(signal);
    }

    public static void Unregister(SignalEmitter signal)
    {
        Signals.Remove(signal);
    }
}