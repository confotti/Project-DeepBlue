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
    public Material leafRenderMaterial;   // material used for leaf instancing
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
    int updateLeafKernel;

    KelpObject[] kelpObjectsCPU;

    [StructLayout(LayoutKind.Sequential)]
    struct StalkNode
    {
        public Vector3 currentPos;
        private float padding0;
        public Vector3 previousPos;
        private float padding1;
        public Vector3 direction;
        private float padding2;
        public Vector4 color;
        public float bendAmount;
        private Vector3 padding3;
        public int isTip;
        private Vector3 padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LeafNode
    {
        public Vector3 currentPos;
        private float padding0;
        public Vector3 previousPos;
        private float padding1;
        public Vector4 color; 
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LeafObject
    {
        public Vector4 orientation;
        public float bendValue;
        public int stalkNodeIndex;
        public int padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KelpObject
    {
        public int startStalkNodeIndex;
        public int stalkNodeCount;
        public int startLeafNodeIndex;
        public int leafNodeCount;
        public Vector3 boundsCenter;
        private float padding0;
        public Vector3 boundsExtents;
        private float padding1;
    }

    void Start()
    {
        InitializeBuffers();
        InitializeData();

        verletKernel = kelpComputeShader.FindKernel("CS_VerletUpdate");
        constraintKernel = kelpComputeShader.FindKernel("CS_ApplyConstraints");
        updateLeafKernel = kelpComputeShader.FindKernel("CS_UpdateLeafNodes");

        if (targetCamera == null) targetCamera = Camera.main;
    }

    void InitializeBuffers()
    {
        // release old
        stalkNodesBuffer?.Release();
        leafNodesBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();

        // create buffers
        stalkNodesBuffer = new ComputeBuffer(totalStalkNodes, Marshal.SizeOf(typeof(StalkNode)));
        // Make leafNodesBuffer the same length as leafObjects (single source of truth)
        leafNodesBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafNode)));
        leafObjectsBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafObject)));
        kelpObjectsBuffer = new ComputeBuffer(totalKelpObjects, Marshal.SizeOf(typeof(KelpObject)));

        kelpObjectsCPU = new KelpObject[totalKelpObjects];
    }

    void InitializeData()
    {
        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        int leavesPerStalk = Mathf.Max(1, totalLeafObjects / totalKelpObjects);

        StalkNode[] stalkNodes = new StalkNode[totalStalkNodes];
        LeafNode[] leafNodes = new LeafNode[totalLeafObjects];      // match count
        LeafObject[] leafObjects = new LeafObject[totalLeafObjects];

        // root positions IN LOCAL SPACE relative to this.transform
        Vector3[] rootPositions = new Vector3[totalKelpObjects];
        for (int i = 0; i < totalKelpObjects; i++)
        {
            float x = Random.Range(-spreadRadius, spreadRadius);
            float z = Random.Range(-spreadRadius, spreadRadius);
            rootPositions[i] = new Vector3(x, 0f, z);
        }

        // debug logs (optional)
        for (int i = 0; i < rootPositions.Length; i++)
            Debug.Log($"Root {i}: local={rootPositions[i]}, world={(transform.position + rootPositions[i])}");

        // initial root buffer (local positions)
        initialRootPositionsBuffer?.Release();
        initialRootPositionsBuffer = new ComputeBuffer(totalKelpObjects, sizeof(float) * 3);
        initialRootPositionsBuffer.SetData(rootPositions);

        // fill kelp objects and nodes
        kelpObjectsCPU = new KelpObject[totalKelpObjects];

        for (int kelpIndex = 0; kelpIndex < totalKelpObjects; kelpIndex++)
        {
            Vector3 baseLocal = rootPositions[kelpIndex];

            kelpObjectsCPU[kelpIndex].startStalkNodeIndex = kelpIndex * nodesPerStalk;
            kelpObjectsCPU[kelpIndex].stalkNodeCount = nodesPerStalk;
            kelpObjectsCPU[kelpIndex].startLeafNodeIndex = kelpIndex * leavesPerStalk;
            kelpObjectsCPU[kelpIndex].leafNodeCount = leavesPerStalk;

            Vector3 centerLocal = baseLocal + Vector3.up * (nodesPerStalk * segmentSpacing * 0.5f);
            Vector3 extents = new Vector3(0.5f, nodesPerStalk * segmentSpacing * 0.5f, 0.5f);

            kelpObjectsCPU[kelpIndex].boundsCenter = centerLocal;
            kelpObjectsCPU[kelpIndex].boundsExtents = extents;

            // stalk nodes
            for (int i = 0; i < nodesPerStalk; i++)
            {
                int nodeIndex = kelpObjectsCPU[kelpIndex].startStalkNodeIndex + i;
                if (nodeIndex >= totalStalkNodes) break;

                Vector3 nodePosLocal = baseLocal + Vector3.up * (i * segmentSpacing);

                stalkNodes[nodeIndex].currentPos = nodePosLocal;
                stalkNodes[nodeIndex].previousPos = nodePosLocal;
                stalkNodes[nodeIndex].direction = Vector3.up;
                stalkNodes[nodeIndex].color = kelpColor;
                stalkNodes[nodeIndex].bendAmount = 0f;
                stalkNodes[nodeIndex].isTip = (i == nodesPerStalk - 1) ? 1 : 0;
            }

            // leaves (CPU-side objects)
            for (int l = 0; l < leavesPerStalk; l++)
            {
                int leafObjIndex = kelpObjectsCPU[kelpIndex].startLeafNodeIndex + l;
                if (leafObjIndex >= totalLeafObjects) break;

                // attach leaf to a stalk node in the same kelp
                leafObjects[leafObjIndex].stalkNodeIndex =
                    kelpObjectsCPU[kelpIndex].startStalkNodeIndex + (l % nodesPerStalk);

                // default orientation (identity quaternion)
                leafObjects[leafObjIndex].orientation = new Vector4(0, 0, 0, 1);
                leafObjects[leafObjIndex].bendValue = 0f;
                leafObjects[leafObjIndex].padding = 0;

                // initialize leaf node so shader has valid defaults (green)
                leafNodes[leafObjIndex].currentPos = baseLocal; // will be overwritten by compute shader anyway
                leafNodes[leafObjIndex].previousPos = baseLocal;
                leafNodes[leafObjIndex].color = new Vector4(0.2f, 0.8f, 0.2f, 1f);
            }
        }

        // set buffer data
        stalkNodesBuffer.SetData(stalkNodes);
        leafNodesBuffer.SetData(leafNodes);
        leafObjectsBuffer.SetData(leafObjects);
        kelpObjectsBuffer.SetData(kelpObjectsCPU);
    }

    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        // set sim params
        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);
        kelpComputeShader.SetFloat("_SegmentSpacing", segmentSpacing);
        kelpComputeShader.SetFloat("_Time", Time.time);
        kelpComputeShader.SetFloat("_WindStrength", windStrength);
        kelpComputeShader.SetFloat("_WindFrequency", windFrequency);

        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        kelpComputeShader.SetInt("_NodesPerStalk", nodesPerStalk);

        // bind buffers for kernels
        kelpComputeShader.SetBuffer(verletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "initialRootPositions", initialRootPositionsBuffer);

        kelpComputeShader.SetBuffer(constraintKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(constraintKernel, "initialRootPositions", initialRootPositionsBuffer);

        kelpComputeShader.SetBuffer(updateLeafKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(updateLeafKernel, "_LeafNodesBuffer", leafNodesBuffer);
        kelpComputeShader.SetBuffer(updateLeafKernel, "_LeafObjectsBuffer", leafObjectsBuffer);

        // dispatch stalk kernels
        int stalkThreadGroups = Mathf.CeilToInt(totalStalkNodes / 64f);
        stalkThreadGroups = Mathf.Max(1, stalkThreadGroups);
        kelpComputeShader.Dispatch(verletKernel, stalkThreadGroups, 1, 1);

        for (int i = 0; i < 25; i++)
            kelpComputeShader.Dispatch(constraintKernel, stalkThreadGroups, 1, 1);

        // dispatch leaf updater (uses totalLeafObjects)
        int leafThreadGroups = Mathf.CeilToInt(totalLeafObjects / 64f);
        leafThreadGroups = Mathf.Max(1, leafThreadGroups);
        kelpComputeShader.Dispatch(updateLeafKernel, leafThreadGroups, 1, 1);

        // Frustum culling could go here (kept as-is)
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);

        // set buffers on materials and world offset
        kelpRenderMaterial.SetVector("_WorldOffset", transform.position);
        kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
        kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

        // IMPORTANT: set leaf buffers on leafRenderMaterial (the one we draw with)
        leafRenderMaterial.SetVector("_WorldOffset", transform.position);
        leafRenderMaterial.SetBuffer("_LeafNodesBuffer", leafNodesBuffer);
        leafRenderMaterial.SetBuffer("_LeafObjectsBuffer", leafObjectsBuffer);

        // draw bounds
        Bounds drawBounds = new Bounds(
            transform.position + Vector3.up * (totalStalkNodes * segmentSpacing * 0.5f),
            new Vector3(spreadRadius * 2f + 10f, totalStalkNodes * segmentSpacing + 10f, spreadRadius * 2f + 10f)
        );

        // Draw stalks
        int instanceCount = totalStalkNodes;
        Graphics.DrawMeshInstancedProcedural(
            kelpSegmentMesh,
            0,
            kelpRenderMaterial,
            drawBounds,
            instanceCount
        );

        // Draw leaves — instance count must match length of _LeafNodesBuffer / _LeafObjectsBuffer
        Graphics.DrawMeshInstancedProcedural(
            kelpLeafMesh,
            0,
            leafRenderMaterial,
            drawBounds,
            totalLeafObjects
        );
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