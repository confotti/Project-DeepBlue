using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OxygenLine : MonoBehaviour
{
    [System.Serializable]
    public struct RopePoint
    {
        public Vector3 position;
        public Vector3 normal;

        public RopePoint(Vector3 pos, Vector3 n)
        {
            position = pos;
            normal = n;
        }
    }

    public Transform player;
    public Transform anchor;

    public float maxRopeLength = 15f;
    public float wrapOffset = 0.05f;

    public float slideSpeed = 10f;
    public float collisionCheckRadius = 0.05f;

    public float unwrapCheckDistance = 0.2f;

    public LayerMask collisionMask;

    private List<RopePoint> ropePoints = new List<RopePoint>();
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        ropePoints.Clear();
        ropePoints.Add(new RopePoint(anchor.position, Vector3.up));
    }

    void Update()
    {
        UpdateRope();
        SlidePoints();
        ConstrainPlayer();
        RenderRope();
    }

    // -------------------------
    // ROPE CORE
    // -------------------------
    void UpdateRope()
    {
        RopePoint last = ropePoints[ropePoints.Count - 1];

        // ---------- WRAPPING ----------
        if (Physics.Linecast(last.position, player.position, out RaycastHit hit, collisionMask))
        {
            Vector3 wrapPoint = SolveEdgePivot(hit, last.position, player.position);

            if (ropePoints.Count < 2 || Vector3.Distance(wrapPoint, last.position) > 0.1f)
            {
                ropePoints.Add(new RopePoint(wrapPoint, hit.normal));
            }
        }

        // ---------- UNWRAPPING ----------
        while (ropePoints.Count > 1)
        {
            RopePoint prev = ropePoints[^2];

            if (!Physics.Linecast(player.position, prev.position, collisionMask))
            {
                ropePoints.RemoveAt(ropePoints.Count - 1);
            }
            else break;
        }

        // ---------- CLEAN STRAIGHT POINTS ----------
        if (ropePoints.Count > 2)
        {
            RopePoint a = ropePoints[^3];
            RopePoint b = ropePoints[^2];
            RopePoint c = ropePoints[^1];

            float angle = Vector3.Angle(b.position - a.position, c.position - b.position);

            if (angle < 5f)
            {
                ropePoints.RemoveAt(ropePoints.Count - 2);
            }
        }
    }

    // -------------------------
    // EDGE PIVOT (UNCHANGED BUT RELIABLE)
    // -------------------------
    Vector3 SolveEdgePivot(RaycastHit hit, Vector3 from, Vector3 to)
    {
        Vector3 toPlayer = (to - hit.point).normalized;
        Vector3 toAnchor = (from - hit.point).normalized;

        Vector3 cornerAxis = Vector3.Cross(toAnchor, toPlayer).normalized;
        Vector3 edgeDir = Vector3.Cross(hit.normal, cornerAxis).normalized;

        if (edgeDir == Vector3.zero)
            edgeDir = Vector3.Cross(hit.normal, Vector3.up).normalized;

        Vector3 best = hit.point;
        float bestDist = float.MaxValue;

        for (float t = -0.5f; t <= 0.5f; t += 0.1f)
        {
            Vector3 test = hit.point + edgeDir * t;

            if (Physics.CheckSphere(test, collisionCheckRadius, collisionMask))
                continue;

            float dist =
                Vector3.Distance(from, test) +
                Vector3.Distance(test, to);

            if (dist < bestDist)
            {
                bestDist = dist;
                best = test;
            }
        }

        return best + hit.normal * wrapOffset;
    }

    // -------------------------
    // 🔥 FIXED SLIDING SYSTEM
    // -------------------------
    void SlidePoints()
    {
        for (int i = 1; i < ropePoints.Count - 1; i++)
        {
            RopePoint p = ropePoints[i];

            Vector3 prev = ropePoints[i - 1].position;
            Vector3 next = ropePoints[i + 1].position;

            Vector3 ropeDir = (next - prev).normalized;

            // Project movement onto stored surface
            Vector3 slideDir = Vector3.ProjectOnPlane(ropeDir, p.normal);

            if (slideDir.sqrMagnitude < 0.0001f)
                continue;

            Vector3 candidate = p.position + slideDir * Time.deltaTime * slideSpeed;

            // prevent clipping
            if (Physics.CheckSphere(candidate, collisionCheckRadius, collisionMask))
                continue;

            float oldDist =
                Vector3.Distance(prev, p.position) +
                Vector3.Distance(p.position, next);

            float newDist =
                Vector3.Distance(prev, candidate) +
                Vector3.Distance(candidate, next);

            if (newDist < oldDist)
            {
                p.position = candidate;
                ropePoints[i] = p;
            }
        }
    }

    // -------------------------
    // LENGTH CONSTRAINT
    // -------------------------
    void ConstrainPlayer()
    {
        float totalLength = 0f;

        for (int i = 0; i < ropePoints.Count - 1; i++)
        {
            totalLength += Vector3.Distance(ropePoints[i].position, ropePoints[i + 1].position);
        }

        RopePoint last = ropePoints[^1];

        totalLength += Vector3.Distance(last.position, player.position);

        if (totalLength > maxRopeLength)
        {
            float excess = totalLength - maxRopeLength;

            Vector3 dir = (player.position - last.position).normalized;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity -= Vector3.Project(rb.linearVelocity, dir);
                rb.position -= dir * excess;
            }
            else
            {
                player.position -= dir * excess;
            }
        }
    }

    // -------------------------
    // RENDER
    // -------------------------
    void RenderRope()
    {
        lineRenderer.positionCount = ropePoints.Count + 1;

        for (int i = 0; i < ropePoints.Count; i++)
        {
            lineRenderer.SetPosition(i, ropePoints[i].position);
        }

        lineRenderer.SetPosition(ropePoints.Count, player.position);
    }
}