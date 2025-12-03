using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Floater : MonoBehaviour
{
    public WaterSurface surface;

    public bool PlayInEditor;

    WaterSearchParameters wsp = new();
    WaterSearchResult wsr = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        wsp.startPositionWS = wsr.candidateLocationWS;
        wsp.targetPositionWS = gameObject.transform.position;
        wsp.error = 0.01f;
        wsp.maxIterations = 8;

        if(surface.ProjectPointOnWaterSurface(wsp, out wsr))
        {
            gameObject.transform.position = wsr.projectedPositionWS;
        }
    }
}
