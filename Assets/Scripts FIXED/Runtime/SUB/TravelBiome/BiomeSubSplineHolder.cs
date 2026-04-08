using System;
using UnityEngine;
using UnityEngine.Splines;

public class BiomeSubSplineHolder : MonoBehaviour
{
    public SplineContainer entrySpline;
    public SplineContainer exitSpline;

    public static event Action<BiomeSubSplineHolder> OnSpawned;

    void Awake()
    {
        OnSpawned?.Invoke(this);
    }

    public void FixExitSpline(Vector3 whereTo)
    {
        //prevInstance.exitSpline.Spline.Add(new BezierKnot(whereTo));
    }
}
