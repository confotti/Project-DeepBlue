using UnityEngine;
using System.Collections.Generic;

public class KelpSimulationGPU : MonoBehaviour
{
    [Header("Kelp Settings")]
    [SerializeField] private int numberOfKelpSegments = 50;
    [SerializeField] private float kelpSegmentLength = 0.225f;
    [SerializeField] private Mesh kelpSegmentMesh;
    [SerializeField] private Material kelpMaterial;

    [Header("Physics")]
    [SerializeField] private Vector3 gravityForce = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private float dampingFactor = 0.98f;

    private List<KelpSegment> kelpSegments = new ();
    private Matrix4x4[] matrices;
    private Vector4[] startPositions;
    private Vector4[] endPositions;
    private Vector4[] prevPositions;
    private Vector4[] nextPositions;

    private MaterialPropertyBlock props;

    private void Awake()
    {
        Vector3 kelpStartPosition = transform.position;

        for (int i = 0; i < numberOfKelpSegments; i++)
        {
            kelpSegments.Add(new KelpSegment(kelpStartPosition));
            kelpStartPosition.y += kelpSegmentLength;
        }

        int instanceCount = numberOfKelpSegments - 1;

        matrices = new Matrix4x4[instanceCount];
        startPositions = new Vector4[instanceCount];
        endPositions = new Vector4[instanceCount];
        prevPositions = new Vector4[instanceCount];
        nextPositions = new Vector4[instanceCount];

        props = new MaterialPropertyBlock();
    }

    private void FixedUpdate()
    {
        Simulate();

        // Pin root segment to GameObject's position
        kelpSegments[0] = new KelpSegment(transform.position);
    }

    private void Update()
    {
        int count = kelpSegments.Count;

        for (int i = 0; i < count - 1; i++)
        {
            var segA = kelpSegments[i];
            var segB = kelpSegments[i + 1];

            startPositions[i] = segA.CurrentPosition;
            endPositions[i] = segB.CurrentPosition;

            // Previous segment for smoothing
            if (i == 0)
                prevPositions[i] = segA.CurrentPosition; // duplicate for first
            else
                prevPositions[i] = kelpSegments[i - 1].CurrentPosition;

            // Next segment for smoothing
            if (i + 2 >= count)
                nextPositions[i] = segB.CurrentPosition; // duplicate for last
            else
                nextPositions[i] = kelpSegments[i + 2].CurrentPosition;

            // Use identity matrix; transformation is handled in shader
            matrices[i] = Matrix4x4.identity;
        }

        // Assign arrays to shader
        props.SetVectorArray("_StartPos", startPositions);
        props.SetVectorArray("_EndPos", endPositions);
        props.SetVectorArray("_PrevPos", prevPositions);
        props.SetVectorArray("_NextPos", nextPositions);

        Graphics.DrawMeshInstanced(kelpSegmentMesh, 0, kelpMaterial, matrices, numberOfKelpSegments - 1, props);
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

        ApplyConstraints();
    }

    private void ApplyConstraints()
    {
        for (int i = 0; i < kelpSegments.Count - 1; i++)
        {
            var segA = kelpSegments[i];
            var segB = kelpSegments[i + 1];

            Vector3 delta = segB.CurrentPosition - segA.CurrentPosition;
            float dist = delta.magnitude;
            float diff = dist - kelpSegmentLength;
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