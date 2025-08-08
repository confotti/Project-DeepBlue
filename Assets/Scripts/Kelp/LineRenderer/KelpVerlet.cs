using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class KelpVerlet : MonoBehaviour
{
    [Header("Kelp")]
    [SerializeField] private int numberOfKelpSegments = 50;
    [SerializeField] private float kelpSegmentLength = 0.225f;

    [Header("Physics")]
    [SerializeField] private Vector3 gravityForce = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float dampingFactor = 0.98f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private float collisionRadius = 0.1f;
    [SerializeField] private float bounceFactor = 0.1f;

    [Header("Constraints")]
    [SerializeField] private int numberOfConstraintRuns = 50;

    [Header("Optimizations")]
    [SerializeField] private int collisionSegmentInterval = 2;

    [Header("Segment Mesh")]
    [SerializeField] private GameObject kelpSegmentMeshPrefab;

    private List<GameObject> segmentMeshes = new ();
    private List<KelpSegment> kelpSegments = new ();
    private Vector3[] kelpPositions;

    private LineRenderer lineRenderer;
    private float sqrCollisionRadius;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = numberOfKelpSegments;
        kelpPositions = new Vector3[numberOfKelpSegments];
        sqrCollisionRadius = collisionRadius * collisionRadius;

        Vector3 kelpStartPosition = transform.position;

        for (int i = 0; i < numberOfKelpSegments; i++)
        {
            kelpSegments.Add(new KelpSegment(kelpStartPosition));

            if (i > 0 && kelpSegmentMeshPrefab != null)
            {
                var segMesh = Instantiate(kelpSegmentMeshPrefab, kelpStartPosition, Quaternion.identity, transform);
                segmentMeshes.Add(segMesh);
            }

            kelpStartPosition.y += kelpSegmentLength;
        }
    }

    private void Update()
    {
        for (int i = 1; i < kelpSegments.Count; i++)
        {
            if (i - 1 < segmentMeshes.Count)
            {
                var pos = kelpSegments[i].CurrentPosition;
                var mesh = segmentMeshes[i - 1];
                mesh.transform.position = pos;

                Vector3 dir = pos - kelpSegments[i - 1].CurrentPosition;
                if (dir.sqrMagnitude > 0.0001f)
                    mesh.transform.rotation = Quaternion.LookRotation(dir);  
            }
        }

        for (int i = 0; i < kelpSegments.Count; i++)
        {
            kelpPositions[i] = transform.InverseTransformPoint(kelpSegments[i].CurrentPosition);
        }

        lineRenderer.SetPositions(kelpPositions);
    }

    private void FixedUpdate()
    {
        Simulate();

        for (int i = 0; i < numberOfConstraintRuns; i++)
        {
            ApplyConstraints();

            if (i % collisionSegmentInterval == 0)
                HandleCollision();
        }

        // Pin top segment to GameObject's transform position
        kelpSegments[0] = new KelpSegment(transform.position);
    }

    private void Simulate()
    {
        float deltaTime = Time.fixedDeltaTime;

        for (int i = 1; i < kelpSegments.Count; i++)
        {
            KelpSegment segment = kelpSegments[i];

            Vector3 velocity = (segment.CurrentPosition - segment.OldPosition) * dampingFactor;
            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += velocity;
            segment.CurrentPosition += gravityForce * deltaTime;

            kelpSegments[i] = segment;
        }
    }

    private void ApplyConstraints()
    {
        for (int i = 0; i < kelpSegments.Count - 1; i++)
        {
            var segA = kelpSegments[i];
            var segB = kelpSegments[i + 1];

            Vector3 delta = segB.CurrentPosition - segA.CurrentPosition;
            float dist = delta.magnitude;
            float diff = (dist - kelpSegmentLength);
            Vector3 correction = delta.normalized * diff;

            if (i != 0)
            {
                segA.CurrentPosition += correction * 0.5f;
                segB.CurrentPosition -= correction * 0.5f;
            }
            else
            {
                segB.CurrentPosition -= correction;
            }

            kelpSegments[i] = segA;
            kelpSegments[i + 1] = segB;
        }
    }

    private void HandleCollision()
    {
        for (int i = 0; i < kelpSegments.Count; i++)
        {
            var segment = kelpSegments[i];
            Vector3 velocity = segment.CurrentPosition - segment.OldPosition;

            Collider[] colliders = Physics.OverlapSphere(segment.CurrentPosition, collisionRadius, collisionMask);

            foreach (var col in colliders)
            {
                Vector3 closest = col.ClosestPoint(segment.CurrentPosition);
                Vector3 toSegment = segment.CurrentPosition - closest;
                float sqrDist = toSegment.sqrMagnitude;

                if (sqrDist < sqrCollisionRadius)
                {
                    Vector3 normal = toSegment.normalized;
                    float depth = collisionRadius - Mathf.Sqrt(sqrDist);

                    if (normal == Vector3.zero)
                        normal = (segment.CurrentPosition - col.transform.position).normalized;

                    segment.CurrentPosition += normal * depth;
                    velocity = Vector3.Reflect(velocity, normal) * bounceFactor;
                }
            }

            segment.OldPosition = segment.CurrentPosition - velocity;
            kelpSegments[i] = segment;
        }
    }

    private struct KelpSegment 
    {
        public Vector3 CurrentPosition;
        public Vector3 OldPosition;

        public KelpSegment(Vector3 pos)
        {
            CurrentPosition = pos;
            OldPosition = pos;
        }
    }
} 