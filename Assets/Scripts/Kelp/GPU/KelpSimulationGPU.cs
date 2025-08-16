using UnityEngine;
using System.Runtime.InteropServices;

public class KelpSimulationGPU_Advanced : MonoBehaviour
{
    [Header("Kelp Settings")]
    public int totalStalkNodes = 1000;
    public int totalLeafObjects = 500;
    public int totalKelpObjects = 50;

    [Header("Physics")]
    public Vector3 gravityForce = new Vector3(0, -9.81f, 0);
    public float damping = 0.98f;

    [Header("References")]
    public ComputeShader kelpComputeShader;
    public Material kelpRenderMaterial;
    public Mesh kelpSegmentMesh;
    public Material leafRenderMaterial;
    public Mesh kelpLeafMesh;
    public Camera targetCamera;

    [Header("Visual Tuning")]
    public float segmentSpacing = 0.1f;
    public Color kelpColor = Color.white;
    public float windStrength = 0.5f;
    public float windFrequency = 1f; 

    // 2–3 nodes per leaf gives nice rope-like motion
    [Range(2, 6)]
    public int leafNodesPerLeaf = 3;

    [Header("Placement")]
    public float spreadRadius = 5f;

    // compute buffers
    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafSegmentsBuffer;   // totalLeafObjects * leafNodesPerLeaf
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;
    ComputeBuffer initialRootPositionsBuffer;

    // kernels
    int stalkVerletKernel;
    int stalkConstraintKernel;
    int leafVerletKernel;          // NEW
    int leafConstraintKernel;      // NEW
    int updateLeavesKernel;        // keeps orientation/bend for rendering

    KelpObject[] kelpObjectsCPU;

    [StructLayout(LayoutKind.Sequential)]
    struct StalkNode
    {
        public Vector3 currentPos; private float pad0;
        public Vector3 previousPos; private float pad1;
        public Vector3 direction; private float pad2;
        public Vector4 color;
        public float bendAmount; private Vector3 pad3;
        public int isTip; private Vector3 pad4;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LeafSegment
    {
        public Vector3 currentPos; private float pad0;
        public Vector3 previousPos; private float pad1;
        public Vector4 color;
    }

    // 16B alignment mirrors HLSL
    [StructLayout(LayoutKind.Sequential)]
    struct LeafObject
    {
        public Vector4 orientation;     // 16
        public Vector3 bendAxis;        // 12
        public float bendAngle;         // 4
        public int stalkNodeIndex;      // 4 (the n0 attachment)
        public float angleAroundStem;   // 4
        public Vector2 pad;             // 8
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KelpObject
    {
        public int startStalkNodeIndex;
        public int stalkNodeCount;
        public int startLeafIndex;      // index into leafObjects
        public int leafCount;
        public Vector3 boundsCenter; private float pad0;
        public Vector3 boundsExtents; private float pad1;
    }

    void Start()
    {
        InitializeBuffers();
        InitializeData();

        // Find kernels
        stalkVerletKernel = kelpComputeShader.FindKernel("CS_VerletUpdate");
        stalkConstraintKernel = kelpComputeShader.FindKernel("CS_ApplyConstraints");
        leafVerletKernel = kelpComputeShader.FindKernel("CS_VerletUpdateLeaves");
        leafConstraintKernel = kelpComputeShader.FindKernel("CS_ApplyLeafConstraints");
        updateLeavesKernel = kelpComputeShader.FindKernel("CS_UpdateLeaves");

        if (targetCamera == null) targetCamera = Camera.main;
    }

    void InitializeBuffers()
    {
        stalkNodesBuffer?.Release();
        leafSegmentsBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();

        stalkNodesBuffer = new ComputeBuffer(totalStalkNodes, Marshal.SizeOf(typeof(StalkNode)));
        // Allocate segments for ALL leaf nodes
        int totalLeafSegments = Mathf.Max(1, totalLeafObjects * Mathf.Max(2, leafNodesPerLeaf));
        leafSegmentsBuffer = new ComputeBuffer(totalLeafSegments, Marshal.SizeOf(typeof(LeafSegment)));
        leafObjectsBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafObject)));
        kelpObjectsBuffer = new ComputeBuffer(totalKelpObjects, Marshal.SizeOf(typeof(KelpObject)));

        kelpObjectsCPU = new KelpObject[totalKelpObjects];
    }

    void InitializeData()
    {
        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        int leavesPerStalk = Mathf.Max(1, totalLeafObjects / totalKelpObjects);
        int totalLeafSegments = totalLeafObjects * Mathf.Max(2, leafNodesPerLeaf);

        var stalkNodes = new StalkNode[totalStalkNodes];
        var leafSegments = new LeafSegment[totalLeafSegments];
        var leafObjs = new LeafObject[totalLeafObjects];

        // local root positions
        Vector3[] rootPositions = new Vector3[totalKelpObjects];
        for (int i = 0; i < totalKelpObjects; i++)
        {
            float x = Random.Range(-spreadRadius, spreadRadius);
            float z = Random.Range(-spreadRadius, spreadRadius);
            rootPositions[i] = new Vector3(x, 0f, z);
        }

        // Fill stalks + leaves
        for (int k = 0; k < totalKelpObjects; k++)
        {
            Vector3 baseLocal = rootPositions[k];

            kelpObjectsCPU[k].startStalkNodeIndex = k * nodesPerStalk;
            kelpObjectsCPU[k].stalkNodeCount = nodesPerStalk;
            kelpObjectsCPU[k].startLeafIndex = k * leavesPerStalk;
            kelpObjectsCPU[k].leafCount = leavesPerStalk;

            Vector3 centerLocal = baseLocal + Vector3.up * (nodesPerStalk * segmentSpacing * 0.5f);
            Vector3 extents = new Vector3(0.5f, nodesPerStalk * segmentSpacing * 0.5f, 0.5f);
            kelpObjectsCPU[k].boundsCenter = centerLocal;
            kelpObjectsCPU[k].boundsExtents = extents;

            // Stalk nodes
            for (int i = 0; i < nodesPerStalk; i++)
            {
                int nodeIndex = kelpObjectsCPU[k].startStalkNodeIndex + i;
                if (nodeIndex >= totalStalkNodes) break;

                Vector3 nodePosLocal = baseLocal + Vector3.up * (i * segmentSpacing);
                stalkNodes[nodeIndex].currentPos = nodePosLocal;
                stalkNodes[nodeIndex].previousPos = nodePosLocal;
                stalkNodes[nodeIndex].direction = Vector3.up;
                stalkNodes[nodeIndex].color = kelpColor;
                stalkNodes[nodeIndex].bendAmount = 0f;
                stalkNodes[nodeIndex].isTip = (i == nodesPerStalk - 1) ? 1 : 0;
            }

            // Leaf objects + mini ropes
            for (int l = 0; l < leavesPerStalk; l++)
            {
                int li = kelpObjectsCPU[k].startLeafIndex + l;
                if (li >= totalLeafObjects) break;

                // Attach to a random stalk segment (not last two)
                int randSegLocal = Mathf.Clamp(Random.Range(0, nodesPerStalk - 2), 0, nodesPerStalk - 3);
                int n0 = kelpObjectsCPU[k].startStalkNodeIndex + randSegLocal;

                leafObjs[li].stalkNodeIndex = n0;
                leafObjs[li].angleAroundStem = Random.Range(0f, Mathf.PI * 2f);
                leafObjs[li].orientation = new Vector4(0, 0, 0, 1);
                leafObjs[li].bendAxis = new Vector3(0, 0, 1);
                leafObjs[li].bendAngle = 0f;
                leafObjs[li].pad = Vector2.zero;

                // Initialize mini chain along a small outward dir from the stalk
                Vector3 n0Pos = stalkNodes[n0].currentPos;
                // Simple outward guess; shader will refine using angleAroundStem
                Vector3 outward = Vector3.right;

                for (int n = 0; n < leafNodesPerLeaf; n++)
                {
                    int segIndex = li * leafNodesPerLeaf + n;

                    // Original upward segment
                    Vector3 p = n0Pos + Vector3.up * (segmentSpacing * (n + 0.25f));

                    // Rotate the node 45 degrees around the stem (Y-axis)
                    float angleRad = Mathf.Deg2Rad * 45f; // 45 degrees in radians
                    Vector3 offset = new Vector3(Mathf.Cos(angleRad), 0, Mathf.Sin(angleRad)) * 0.02f;
                    p += offset;

                    leafSegments[segIndex].currentPos = p;
                    leafSegments[segIndex].previousPos = p;
                    leafSegments[segIndex].color = new Vector4(0.2f, 0.8f, 0.2f, 1f);
                } 
            }
        }

        // upload static data
        stalkNodesBuffer.SetData(stalkNodes);
        leafSegmentsBuffer.SetData(leafSegments);
        leafObjectsBuffer.SetData(leafObjs);
        kelpObjectsBuffer.SetData(kelpObjectsCPU);

        // roots buffer
        initialRootPositionsBuffer?.Release();
        initialRootPositionsBuffer = new ComputeBuffer(totalKelpObjects, sizeof(float) * 3);
        initialRootPositionsBuffer.SetData(rootPositions);
    }

    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);
        kelpComputeShader.SetFloat("_SegmentSpacing", segmentSpacing);
        kelpComputeShader.SetFloat("_Time", Time.time);
        kelpComputeShader.SetFloat("_WindStrength", windStrength);
        kelpComputeShader.SetFloat("_WindFrequency", windFrequency);

        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        int totalLeafSegments = totalLeafObjects * Mathf.Max(2, leafNodesPerLeaf);

        kelpComputeShader.SetInt("_NodesPerStalk", nodesPerStalk);
        kelpComputeShader.SetInt("_LeafNodesPerLeaf", Mathf.Max(2, leafNodesPerLeaf));
        kelpComputeShader.SetInt("_TotalLeafObjects", totalLeafObjects);

        // --- Bind stalk buffers
        kelpComputeShader.SetBuffer(stalkVerletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(stalkVerletKernel, "initialRootPositions", initialRootPositionsBuffer);

        kelpComputeShader.SetBuffer(stalkConstraintKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(stalkConstraintKernel, "initialRootPositions", initialRootPositionsBuffer);

        // --- Bind leaf buffers (verlet + constraints)
        kelpComputeShader.SetBuffer(leafVerletKernel, "_LeafSegmentsBuffer", leafSegmentsBuffer);
        kelpComputeShader.SetBuffer(leafVerletKernel, "_LeafObjectsBuffer", leafObjectsBuffer);
        kelpComputeShader.SetBuffer(leafVerletKernel, "_StalkNodesBuffer", stalkNodesBuffer);

        kelpComputeShader.SetBuffer(leafConstraintKernel, "_LeafSegmentsBuffer", leafSegmentsBuffer);
        kelpComputeShader.SetBuffer(leafConstraintKernel, "_LeafObjectsBuffer", leafObjectsBuffer);
        kelpComputeShader.SetBuffer(leafConstraintKernel, "_StalkNodesBuffer", stalkNodesBuffer);

        // --- Bind for leaf orientation update (instances)
        kelpComputeShader.SetBuffer(updateLeavesKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(updateLeavesKernel, "_LeafSegmentsBuffer", leafSegmentsBuffer);
        kelpComputeShader.SetBuffer(updateLeavesKernel, "_LeafObjectsBuffer", leafObjectsBuffer);

        // --- Dispatch stalk
        int stalkGroups = Mathf.Max(1, Mathf.CeilToInt(totalStalkNodes / 64f));
        kelpComputeShader.Dispatch(stalkVerletKernel, stalkGroups, 1, 1);
        for (int i = 0; i < 25; i++) kelpComputeShader.Dispatch(stalkConstraintKernel, stalkGroups, 1, 1); 

        // --- Dispatch leaves (verlet over segments, then constraints over segments, then per-leaf update)
        int leafSegGroups = Mathf.Max(1, Mathf.CeilToInt(totalLeafSegments / 64f));
        kelpComputeShader.Dispatch(leafVerletKernel, leafSegGroups, 1, 1);
        for (int i = 0; i < 25; i++) kelpComputeShader.Dispatch(leafConstraintKernel, leafSegGroups, 1, 1);

        // one thread per LEAF (not per segment) to update orientation/bend/around-stem
        int leafGroups = Mathf.Max(1, Mathf.CeilToInt(totalLeafObjects / 64f));
        kelpComputeShader.Dispatch(updateLeavesKernel, leafGroups, 1, 1);

        // Materials
        kelpRenderMaterial.SetVector("_WorldOffset", transform.position);
        kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
        kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

        leafRenderMaterial.SetVector("_WorldOffset", transform.position);
        leafRenderMaterial.SetInt("_LeafNodesPerLeaf", Mathf.Max(2, leafNodesPerLeaf));
        leafRenderMaterial.SetBuffer("_LeafSegmentsBuffer", leafSegmentsBuffer);
        leafRenderMaterial.SetBuffer("_LeafObjectsBuffer", leafObjectsBuffer);

        // Draw
        Bounds drawBounds = new Bounds(
            transform.position + Vector3.up * (totalStalkNodes * segmentSpacing * 0.5f),
            new Vector3(spreadRadius * 2f + 10f, totalStalkNodes * segmentSpacing + 10f, spreadRadius * 2f + 10f)
        );

        Graphics.DrawMeshInstancedProcedural(kelpSegmentMesh, 0, kelpRenderMaterial, drawBounds, totalStalkNodes);
        Graphics.DrawMeshInstancedProcedural(kelpLeafMesh, 0, leafRenderMaterial, drawBounds, totalLeafObjects);
    }

    void OnDestroy()
    {
        stalkNodesBuffer?.Release();
        leafSegmentsBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();
    }
} 