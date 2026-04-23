using System.Collections.Generic;
using UnityEngine;

public class OxygenLine : MonoBehaviour
{
    [Header("References")]
    public Transform anchor;
    public Transform player;
    public LineRenderer lineRenderer;

    [Header("Rope Settings")]
    public int segmentCount = 30;
    public float ropeLength = 10f;
    public float gravity = -2f;
    public int solverIterations = 10;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float surfaceOffset = 0.02f;
    public float slideSpeed = 5f;

    private class RopePoint
    {
        public Vector3 position;
        public Vector3 previousPosition;
    }

    private class ContactPoint
    {
        public Vector3 position;
        public Vector3 normal;
    }

    private List<RopePoint> points = new List<RopePoint>();
    private List<ContactPoint> contacts = new List<ContactPoint>();

    float segmentLength;

    void Start()
    {
        segmentLength = ropeLength / segmentCount;

        Vector3 start = anchor.position;
        Vector3 end = player.position;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = Vector3.Lerp(start, end, i / (float)(segmentCount - 1));
            points.Add(new RopePoint
            {
                position = pos,
                previousPosition = pos
            });
        }
    }

    void Update()
    {
        Simulate();
        HandleWrapping();
        SolveConstraints();
        ApplyMaxLength();
        Render();
    }

    void Simulate()
    {
        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector3 velocity = points[i].position - points[i].previousPosition;
            points[i].previousPosition = points[i].position;

            points[i].position += velocity;
            points[i].position += Vector3.up * gravity * Time.deltaTime;
        }
    }

    void SolveConstraints()
    {
        for (int iter = 0; iter < solverIterations; iter++)
        {
            // Anchor fixed
            points[0].position = anchor.position;

            // Player attached
            points[points.Count - 1].position = player.position;

            // Segment length constraints
            for (int i = 0; i < points.Count - 1; i++)
            {
                var p1 = points[i];
                var p2 = points[i + 1];

                float dist = Vector3.Distance(p1.position, p2.position);
                float error = dist - segmentLength;

                Vector3 changeDir = (p2.position - p1.position).normalized;
                Vector3 change = changeDir * error * 0.5f;

                if (i != 0)
                    p1.position += change;

                if (i + 1 != points.Count - 1)
                    p2.position -= change;
            }

            // Handle sliding contacts
            foreach (var contact in contacts)
            {
                SlideContact(contact);
            }
        }
    }

    void SlideContact(ContactPoint contact)
    {
        // Find closest rope point to this contact
        RopePoint closest = null;
        float minDist = float.MaxValue;

        foreach (var p in points)
        {
            float d = Vector3.Distance(p.position, contact.position);
            if (d < minDist)
            {
                minDist = d;
                closest = p;
            }
        }

        if (closest == null) return;

        Vector3 toPrev = (anchor.position - contact.position).normalized;
        Vector3 toNext = (player.position - contact.position).normalized;

        Vector3 pull = toPrev + toNext;

        // Project onto surface = sliding
        Vector3 slideDir = Vector3.ProjectOnPlane(pull, contact.normal);

        contact.position += slideDir * slideSpeed * Time.deltaTime;

        // Keep it on surface
        contact.position += contact.normal * surfaceOffset;
    }

    void HandleWrapping()
    {
        contacts.Clear();

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 start = points[i].position;
            Vector3 end = points[i + 1].position;

            Vector3 dir = (end - start).normalized;
            float dist = Vector3.Distance(start, end);

            if (Physics.Raycast(start, dir, out RaycastHit hit, dist, collisionMask))
            {
                ContactPoint cp = new ContactPoint();
                cp.normal = hit.normal;
                cp.position = hit.point + hit.normal * surfaceOffset;

                contacts.Add(cp);
            }
        }
    }

    void ApplyMaxLength()
    {
        float total = 0f;

        for (int i = 0; i < points.Count - 1; i++)
        {
            total += Vector3.Distance(points[i].position, points[i + 1].position);
        }

        if (total > ropeLength)
        {
            Vector3 dir = (player.position - anchor.position).normalized;
            player.position = anchor.position + dir * ropeLength;
        }
    }

    void Render()
    {
        lineRenderer.positionCount = points.Count;

        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i].position);
        }
    }
}