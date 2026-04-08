using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Floater : MonoBehaviour
{
    public WaterSurface surface;

    WaterSearchParameters wsp = new();
    WaterSearchResult wsr = new();

    public bool usingGravity = false;

    // Update is called once per frame
    void Update()
    {
        wsp.startPositionWS = wsr.candidateLocationWS;
        wsp.targetPositionWS = gameObject.transform.position;
        wsp.error = 0.01f;
        wsp.maxIterations = 8;

        if (surface.ProjectPointOnWaterSurface(wsp, out wsr))
        {
            if (usingGravity)
            {
                if (gameObject.transform.position.y < wsr.projectedPositionWS.y) gameObject.transform.position = wsr.projectedPositionWS;
            }

            else
            {
                gameObject.transform.position = wsr.projectedPositionWS;
            }
        }
    }
}
