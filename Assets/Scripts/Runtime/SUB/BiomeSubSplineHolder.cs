using UnityEngine;
using UnityEngine.Splines;

public class BiomeSubSplineHolder : MonoBehaviour
{
    public static BiomeSubSplineHolder nextInstance;
    public static BiomeSubSplineHolder prevInstance;


    public Spline entrySpline;
    public Spline exitSpline;


    private void Awake()
    {
        prevInstance = nextInstance;
        nextInstance = this;
    }

    public void FixExitSpline(Vector3 whereTo)
    {

    }
}
