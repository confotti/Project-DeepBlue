using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OxygenLine : MonoBehaviour
{
    [Header("References")]
    public Transform anchor;
    public Transform player;

    private LineRenderer lineRenderer;

    [Header("Rope Settings")]
    public float maxLength = 15f;
    public float surfaceOffset = 0.02f;

    [Header("Sliding")]
    public float slideSpeed = 5f;

    [Header("Collision")]
    public LayerMask collisionMask;

    private class WrapPoint
    {
        public Vector3 position;
        public Vector3 normal;
    }

    private List<WrapPoint> wrapPoints = new List<WrapPoint>();

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        HandleWrapping();
        SlideWrapPoints();   // ⭐ NEW
        HandleUnwrapping();
        ApplyLengthConstraint();
        Render();
    }

    // =========================
    // WRAPPING
    // =========================
    void HandleWrapping()
    {
        Vector3 from = anchor.position;

        for (int i = 0; i <= wrapPoints.Count; i++)
        {
            Vector3 to = (i == wrapPoints.Count) ? player.position : wrapPoints[i].position;

            Vector3 dir = to - from;
            float dist = dir.magnitude;

            if (dist < 0.001f)
            {
                from = to;
                continue;
            }

            dir /= dist;

            if (Physics.Raycast(from, dir, out RaycastHit hit, dist, collisionMask))
            {
                Vector3 newPoint = hit.point + hit.normal * surfaceOffset;

                if (wrapPoints.Count == 0 ||
                    Vector3.Distance(wrapPoints[wrapPoints.Count - 1].position, newPoint) > 0.1f)
                {
                    wrapPoints.Add(new WrapPoint
                    {
                        position = newPoint,
                        normal = hit.normal
                    });

                    return; // one per frame
                }
            }

            from = to;
        }
    }

    // =========================
    // ⭐ SLIDING ALONG SURFACE
    // =========================
    void SlideWrapPoints()
    {
        for (int i = 0; i < wrapPoints.Count; i++)
        {
            Vector3 prev = (i == 0) ? anchor.position : wrapPoints[i - 1].position;
            Vector3 next = (i == wrapPoints.Count - 1) ? player.position : wrapPoints[i + 1].position;

            WrapPoint wp = wrapPoints[i];

            // Direction rope is pulling this point
            Vector3 pull = (prev - wp.position).normalized + (next - wp.position).normalized;

            // Project onto surface = sliding direction
            Vector3 slideDir = Vector3.ProjectOnPlane(pull, wp.normal).normalized;

            // Move along surface
            wp.position += slideDir * slideSpeed * Time.deltaTime;

            // Re-stick to surface
            if (Physics.Raycast(wp.position - wp.normal, wp.normal, out RaycastHit hit, 0.5f, collisionMask))
            {
                wp.position = hit.point + hit.normal * surfaceOffset;
                wp.normal = hit.normal;
            }
        }
    }

    // =========================
    // UNWRAPPING
    // =========================
    void HandleUnwrapping()
    {
        if (wrapPoints.Count == 0)
            return;

        WrapPoint last = wrapPoints[wrapPoints.Count - 1];

        Vector3 prev = (wrapPoints.Count > 1)
            ? wrapPoints[wrapPoints.Count - 2].position
            : anchor.position;

        if (!Physics.Linecast(prev, player.position, collisionMask))
        {
            wrapPoints.RemoveAt(wrapPoints.Count - 1);
        }
    }

    // =========================
    // LENGTH CONSTRAINT
    // =========================
    void ApplyLengthConstraint()
    {
        float total = 0f;
        Vector3 prev = anchor.position;

        foreach (var wp in wrapPoints)
        {
            total += Vector3.Distance(prev, wp.position);
            prev = wp.position;
        }

        total += Vector3.Distance(prev, player.position);

        if (total > maxLength)
        {
            float remaining = maxLength;

            Vector3 tempPrev = anchor.position;

            foreach (var wp in wrapPoints)
            {
                float seg = Vector3.Distance(tempPrev, wp.position);
                remaining -= seg;
                tempPrev = wp.position;
            }

            remaining = Mathf.Max(0, remaining);

            Vector3 dir = (player.position - prev).normalized;
            player.position = prev + dir * remaining;
        }
    }

    // =========================
    // RENDER
    // =========================
    void Render()
    {
        List<Vector3> pts = new List<Vector3>();

        pts.Add(anchor.position);

        foreach (var wp in wrapPoints)
            pts.Add(wp.position);

        pts.Add(player.position);

        lineRenderer.positionCount = pts.Count;

        for (int i = 0; i < pts.Count; i++)
        {
            lineRenderer.SetPosition(i, pts[i]);
        }
    }
}