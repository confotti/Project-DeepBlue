using UnityEngine;
using UnityEngine.Splines;

public class BiomeSubSplineHolder : MonoBehaviour
{
    public static BiomeSubSplineHolder nextInstance;
    public static BiomeSubSplineHolder prevInstance;


    public SplineContainer entrySpline;
    public SplineContainer exitSpline;


    private void Awake()
    {
        prevInstance = nextInstance;
        nextInstance = this;

        prevInstance.FixExitSpline(nextInstance.transform.TransformPoint(nextInstance.entrySpline.Spline[0].Position));
    }

    public void FixExitSpline(Vector3 whereTo)
    {
        prevInstance.exitSpline.Spline.Add(new BezierKnot(whereTo));
    }
}
