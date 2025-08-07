using UnityEngine;

public class KelpSegmentAnimator : MonoBehaviour
{
    [HideInInspector] public Transform previousSegment;
    [HideInInspector] public Transform nextSegment;

    [Header("Sway Settings")]
    public float swayAmplitude = 10f;
    public float swaySpeed = 1f;
    public float phaseOffset = 0.5f;
    private int index;

    public void Initialize(int i)
    {
        index = i;
    }

    void Update()
    {
        if (previousSegment == null || nextSegment == null) return;

        // Smooth bending
        Vector3 toPrev = (transform.position - previousSegment.position).normalized;
        Vector3 toNext = (nextSegment.position - transform.position).normalized;
        Vector3 averageDir = (toPrev + toNext).normalized;

        if (averageDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(averageDir) * Quaternion.Euler(-90, 0, 0);

        float sway = Mathf.Sin(Time.time * swaySpeed + index * phaseOffset) * swayAmplitude;
        transform.localRotation *= Quaternion.Euler(0, sway, 0); // or change axis here 
    }
} 