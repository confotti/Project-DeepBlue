using System.Collections.Generic;
using UnityEngine;

public class SignalUI : MonoBehaviour
{
    [SerializeField] private SignalPort signalPort;

    [SerializeField] private SonarClamps sonarClampsPrefab;

    [SerializeField] private Color weakSignalColor = Color.white;
    [SerializeField] private Color fullSignalColor = Color.green;

    private List<SonarClamps> sonarClampsList = new();

    private void OnEnable()
    {
        signalPort.signalStrengths += SetClampPositions;
    }

    private void OnDisable()
    {
        
    }

    private void SetClampPositions(float[] strengths)
    {
        for(int i = sonarClampsList.Count - 1; i >= 0; i--)
        {
            ObjectPoolManager.ReturnObjectToPool(sonarClampsList[i].gameObject);
        }
        sonarClampsList.Clear();

        foreach (var signal in strengths)
        {
            var clamps = ObjectPoolManager.SpawnObject(sonarClampsPrefab, transform);
            sonarClampsList.Add(clamps);

            clamps.SetOffset(Mathf.Lerp(800, 20, signal));

            if (signal >= 1) clamps.SetColor(fullSignalColor);
            else clamps.SetColor(weakSignalColor);
        }
    }
}
