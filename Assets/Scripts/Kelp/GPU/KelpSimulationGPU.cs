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

    [Header("Placement")]
    public float spreadRadius = 5f;

    // compute buffers
    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafNodesBuffer;
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;
    ComputeBuffer initialRootPositionsBuffer;

    int verletKernel;
    int constraintKernel;
    int updateLeavesKernel; // CHANGED

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
    struct LeafNode
    {
        public Vector3 currentPos; private float pad0;
        public Vector3 previousPos; private float pad1;
        public Vector4 color;
    }

    // *** UNIFIED LAYOUT with HLSL (16-byte aligned fields) ***
    [StructLayout(LayoutKind.Sequential)]
    struct LeafObject
    {
        public Vector4 orientation;     // 16 bytes
        public Vector3 bendAxis;        // 12
        public float bendAngle;         // 4  -> (aligned to 16)
        public int stalkNodeIndex;      // 4
        public float angleAroundStem;   // 4
        public Vector2 pad;             // 8 to keep 16B alignment
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KelpObject
    {
        public int startStalkNodeIndex;
        public int stalkNodeCount;
        public int startLeafNodeIndex;
        public int leafNodeCount;
        public Vector3 boundsCenter; private float pad0;
        public Vector3 boundsExtents; private float pad1;
    }

    void Start()
    {
        InitializeBuffers();
        InitializeData();

        verletKernel = kelpComputeShader.FindKernel("CS_VerletUpdate");
        constraintKernel = kelpComputeShader.FindKernel("CS_ApplyConstraints");
        updateLeavesKernel = kelpComputeShader.FindKernel("CS_UpdateLeaves"); // CHANGED

        if (targetCamera == null) targetCamera = Camera.main;
    }

    void InitializeBuffers()
    {
        stalkNodesBuffer?.Release();
        leafNodesBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();

        stalkNodesBuffer = new ComputeBuffer(totalStalkNodes, Marshal.SizeOf(typeof(StalkNode)));
        leafNodesBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafNode)));
        leafObjectsBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafObject)));
        kelpObjectsBuffer = new ComputeBuffer(totalKelpObjects, Marshal.SizeOf(typeof(KelpObject)));

        kelpObjectsCPU = new KelpObject[totalKelpObjects];
    }

    void InitializeData()
    {
        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        int leavesPerStalk = Mathf.Max(1, totalLeafObjects / totalKelpObjects);

        var stalkNodes = new StalkNode[totalStalkNodes];
        var leafNodes = new LeafNode[totalLeafObjects];
        var leafObjs = new LeafObject[totalLeafObjects];

        // local root positions
        Vector3[] rootPositions = new Vector3[totalKelpObjects];
        for (int i = 0; i < totalKelpObjects; i++)
        {
            float x = Random.Range(-spreadRadius, spreadRadius);
            float z = Random.Range(-spreadRadius, spreadRadius);
            rootPositions[i] = new Vector3(x, 0f, z);
        }

        initialRootPositionsBuffer?.Release();
        initialRootPositionsBuffer = new ComputeBuffer(totalKelpObjects, sizeof(float) * 3);
        initialRootPositionsBuffer.SetData(rootPositions);

        for (int k = 0; k < totalKelpObjects; k++)
        {
            Vector3 baseLocal = rootPositions[k];

            kelpObjectsCPU[k].startStalkNodeIndex = k * nodesPerStalk;
            kelpObjectsCPU[k].stalkNodeCount = nodesPerStalk;
            kelpObjectsCPU[k].startLeafNodeIndex = k * leavesPerStalk;
            kelpObjectsCPU[k].leafNodeCount = leavesPerStalk;

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

            // Leaf objects/nodes
            for (int l = 0; l < leavesPerStalk; l++)
            {
                int li = kelpObjectsCPU[k].startLeafNodeIndex + l;
                if (li >= totalLeafObjects) break;

                // Attach to a random stalk segment (not the tip-2 border)
                int randSeg = Mathf.Clamp(Random.Range(0, nodesPerStalk - 2), 0, nodesPerStalk - 3);
                leafObjs[li].stalkNodeIndex = kelpObjectsCPU[k].startStalkNodeIndex + randSeg;
                leafObjs[li].angleAroundStem = Random.Range(0f, Mathf.PI * 2f);
                leafObjs[li].orientation = new Vector4(0, 0, 0, 1);
                leafObjs[li].bendAxis = new Vector3(0, 0, 1);
                leafObjs[li].bendAngle = 0f;
                leafObjs[li].pad = Vector2.zero;

                // Init leaf node near base; compute will move it each frame
                Vector3 p = baseLocal;
                leafNodes[li].currentPos = p;
                leafNodes[li].previousPos = p;
                leafNodes[li].color = new Vector4(0.2f, 0.8f, 0.2f, 1f);
            }
        }

        stalkNodesBuffer.SetData(stalkNodes);
        leafNodesBuffer.SetData(leafNodes);
        leafObjectsBuffer.SetData(leafObjs);
        kelpObjectsBuffer.SetData(kelpObjectsCPU);
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
        kelpComputeShader.SetInt("_NodesPerStalk", nodesPerStalk);

        // Bind buffers
        kelpComputeShader.SetBuffer(verletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "initialRootPositions", initialRootPositionsBuffer);

        kelpComputeShader.SetBuffer(constraintKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(constraintKernel, "initialRootPositions", initialRootPositionsBuffer);

        kelpComputeShader.SetBuffer(updateLeavesKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(updateLeavesKernel, "_LeafNodesBuffer", leafNodesBuffer);
        kelpComputeShader.SetBuffer(updateLeavesKernel, "_LeafObjectsBuffer", leafObjectsBuffer);

        // Dispatch stalk
        int stalkGroups = Mathf.Max(1, Mathf.CeilToInt(totalStalkNodes / 64f));
        kelpComputeShader.Dispatch(verletKernel, stalkGroups, 1, 1);
        for (int i = 0; i < 30; i++) kelpComputeShader.Dispatch(constraintKernel, stalkGroups, 1, 1);

        // Dispatch leaves (updates both objects+nodes)
        int leafGroups = Mathf.Max(1, Mathf.CeilToInt(totalLeafObjects / 64f));
        kelpComputeShader.Dispatch(updateLeavesKernel, leafGroups, 1, 1);

        // Materials
        kelpRenderMaterial.SetVector("_WorldOffset", transform.position);
        kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
        kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

        leafRenderMaterial.SetVector("_WorldOffset", transform.position);
        leafRenderMaterial.SetBuffer("_LeafNodesBuffer", leafNodesBuffer);
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
        leafNodesBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();
    }
} 