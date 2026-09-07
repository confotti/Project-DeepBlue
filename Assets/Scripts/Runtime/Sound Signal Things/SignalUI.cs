using System.Collections.Generic;
using UnityEngine;

public class SignalUI : MonoBehaviour
{
    [SerializeField] private SignalPort signalPort;

    [SerializeField] private SonarClamps sonarClampsPrefab;

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
            clamps.SetOffset(Mathf.Lerp(10, 500, 1-signal)); //Kan inte bara sätta till 20, måste räknas ut med strengths. 
        }
    }
}
