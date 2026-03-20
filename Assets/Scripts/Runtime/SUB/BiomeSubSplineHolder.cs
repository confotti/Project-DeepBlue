using UnityEngine;
using UnityEngine.Splines;

public class BiomeSubSplineHolder : MonoBehaviour
{
    public SplineContainer entrySpline;
    public SplineContainer exitSpline;

    public void FixExitSpline(Vector3 whereTo)
    {
        //prevInstance.exitSpline.Spline.Add(new BezierKnot(whereTo));
    }
}
