using UnityEngine;
using System.Runtime.InteropServices;

public class Kelp : MonoBehaviour
{
    [Header("Rendering")]
    public Mesh segmentMesh;
    public Material kelpMaterial;
    public int segmentCount = 10;
    public float segmentSpacing = 0.25f;

    private ComputeBuffer positionBuffer;

    // Match this with shader
    [StructLayout(LayoutKind.Sequential)]
    struct SegmentData
    {
        public Vector3 position;
    }

    void Start()
    {
        // Initialize buffer with segment positions
        SegmentData[] segments = new SegmentData[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segments[i].position = transform.position + Vector3.up * segmentSpacing * i;
        }

        positionBuffer = new ComputeBuffer(segmentCount, Marshal.SizeOf(typeof(SegmentData)));
        positionBuffer.SetData(segments);

        // Send buffer to material
        kelpMaterial.SetBuffer("_SegmentPositions", positionBuffer);
    }

    void Update()
    {
        // Draw the mesh procedurally using instancing
        Graphics.DrawMeshInstancedProcedural(
            segmentMesh,
            0,
            kelpMaterial,
            new Bounds(transform.position, Vector3.one * 10),
            segmentCount
        );
    }

    void OnDestroy()
    {
        if (positionBuffer != null)
        {
            positionBuffer.Release();
            positionBuffer = null;
        }
    }
}
