using UnityEngine;

public class KelpInstancing : MonoBehaviour
{
    [Header("Instance Settings")]
    public int kelpCount = 100;              // Number of full kelp stalks
    public int segmentsPerKelp = 10;         // Segments per kelp stalk
    public Mesh instanceMesh;                // Segment mesh (cut cube or similar)
    public Material instanceMaterial;
    public int subMeshIndex = 0;

    private int totalInstances => kelpCount * segmentsPerKelp;

    private ComputeBuffer instanceBuffer;    // Holds per-segment data
    private ComputeBuffer nodeBuffer;        // Holds kelp nodes (for deformation)
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5];

    private Vector3[] nodes;                 // All nodes (kelpCount * (segmentsPerKelp + 1))
    private Vector3[] kelpOrigins;           // Random positions for each stalk
    private float[] kelpHeights;             // Random height for each stalk

    struct SegmentData
    {
        public Vector3 basePosition;     // World-space base of kelp
        public float kelpID;             // Which kelp this segment belongs to
        public float segmentIndex;       // Which segment in the stalk
        public float segmentCount;       // Total segments
        public float padding;            // Padding to align to 32 bytes
    }

    void Start()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        InitKelpData();
    }

    void Update()
    {
        UpdateNodeBuffer();

        Graphics.DrawMeshInstancedIndirect(
            instanceMesh,
            subMeshIndex,
            instanceMaterial,
            new Bounds(Vector3.zero, new Vector3(500, 500, 500)),
            argsBuffer
        );
    }

    void InitKelpData()
    {
        instanceBuffer?.Release();
        nodeBuffer?.Release();

        SegmentData[] segments = new SegmentData[totalInstances];
        nodes = new Vector3[kelpCount * (segmentsPerKelp + 1)];
        kelpOrigins = new Vector3[kelpCount];
        kelpHeights = new float[kelpCount];

        for (int k = 0; k < kelpCount; k++)
        {
            Vector3 kelpOrigin = new Vector3(
                Random.Range(-100f, 100f),
                0f,
                Random.Range(-100f, 100f)
            );

            kelpOrigins[k] = kelpOrigin;

            float stalkHeight = Random.Range(3f, 7f);
            kelpHeights[k] = stalkHeight;
            float segmentHeight = stalkHeight / segmentsPerKelp;

            // Segment data (used per instance)
            for (int s = 0; s < segmentsPerKelp; s++)
            {
                int index = k * segmentsPerKelp + s;
                segments[index] = new SegmentData
                {
                    basePosition = kelpOrigin,
                    kelpID = k,
                    segmentIndex = s,
                    segmentCount = segmentsPerKelp,
                    padding = 0
                };
            }

            // Node positions for deformation (segmentCount + 1 nodes)
            for (int s = 0; s <= segmentsPerKelp; s++)
            {
                float y = s * segmentHeight;
                float wave = Mathf.Sin(Time.time * 1.5f + s * 0.3f + k) * 0.2f;
                nodes[k * (segmentsPerKelp + 1) + s] = kelpOrigin + new Vector3(wave, y, 0f);
            }
        }

        instanceBuffer = new ComputeBuffer(totalInstances, sizeof(float) * 8);
        instanceBuffer.SetData(segments);
        instanceMaterial.SetBuffer("_InstanceBuffer", instanceBuffer);

        nodeBuffer = new ComputeBuffer(nodes.Length, sizeof(float) * 3);
        instanceMaterial.SetBuffer("_NodeBuffer", nodeBuffer);

        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)totalInstances;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }

        argsBuffer.SetData(args);
    }

    void UpdateNodeBuffer()
    {
        for (int k = 0; k < kelpCount; k++)
        {
            float stalkHeight = kelpHeights[k];
            float segmentHeight = stalkHeight / segmentsPerKelp;
            Vector3 origin = kelpOrigins[k];

            for (int s = 0; s <= segmentsPerKelp; s++)
            {
                float y = s * segmentHeight;
                float wave = Mathf.Sin(Time.time * 1.5f + s * 0.3f + k * 1.1f) * 0.2f;
                nodes[k * (segmentsPerKelp + 1) + s] = origin + new Vector3(wave, y, 0f);
            }
        }

        nodeBuffer.SetData(nodes);
    }

    void OnDisable()
    {
        instanceBuffer?.Release();
        nodeBuffer?.Release();
        argsBuffer?.Release();
    }
} 