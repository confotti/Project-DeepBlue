using UnityEngine;
using System.Runtime.InteropServices;

public class KelpSimulationGPU_Advanced : MonoBehaviour
{
    [Header("Kelp Settings")]
    public int totalStalkNodes = 1000;
    public int totalLeafNodes = 2000;
    public int totalLeafObjects = 500;
    public int totalKelpObjects = 50;

    [Header("Physics")]
    public Vector3 gravityForce = new Vector3(0, -9.81f, 0);
    public float damping = 0.98f;

    [Header("References")]
    public ComputeShader kelpComputeShader;
    public Material kelpRenderMaterial;
    public Mesh kelpSegmentMesh;
    public Camera targetCamera;

    [Header("Visual Tuning")]
    public float segmentSpacing = 0.1f;
    public Color kelpColor = Color.white;
    public float windStrength = 0.5f;
    public float windFrequency = 1f;

    [Header("Placement")]
    public float spreadRadius = 5f; 

    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafNodesBuffer;
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;
    ComputeBuffer initialRootPositionsBuffer; 

    int verletKernel;
    int constraintKernel;

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
        public Vector3 previousPos;
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

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void InitializeBuffers()
    {
        stalkNodesBuffer?.Release();
        leafNodesBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();

        stalkNodesBuffer = new ComputeBuffer(totalStalkNodes, Marshal.SizeOf(typeof(StalkNode)));
        leafNodesBuffer = new ComputeBuffer(totalLeafNodes, Marshal.SizeOf(typeof(LeafNode)));
        leafObjectsBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafObject)));
        kelpObjectsBuffer = new ComputeBuffer(totalKelpObjects, Marshal.SizeOf(typeof(KelpObject)));

        kelpObjectsCPU = new KelpObject[totalKelpObjects];
    }

    void InitializeData()
    {
        // defensive guards
        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        int leavesPerStalk = Mathf.Max(1, totalLeafObjects / totalKelpObjects);

        StalkNode[] stalkNodes = new StalkNode[totalStalkNodes];
        LeafNode[] leafNodes = new LeafNode[totalLeafNodes];
        LeafObject[] leafObjects = new LeafObject[totalLeafObjects];

        // root positions IN LOCAL SPACE relative to this.transform
        Vector3[] rootPositions = new Vector3[totalKelpObjects];
        for (int i = 0; i < totalKelpObjects; i++)
        {
            float x = Random.Range(-spreadRadius, spreadRadius);
            float z = Random.Range(-spreadRadius, spreadRadius);
            // **local** position relative to transform (so shader + _WorldOffset works correctly)
            rootPositions[i] = new Vector3(x, 0f, z);
        }

        // --- DEBUG: log the roots (local and world) --- 
        for (int i = 0; i < rootPositions.Length; i++)
            Debug.Log($"Root {i}: local={rootPositions[i]}, world={(transform.position + rootPositions[i])}");

        // compute buffer for initial root positions (local space)
        if (initialRootPositionsBuffer != null)
            initialRootPositionsBuffer.Release();
        initialRootPositionsBuffer = new ComputeBuffer(totalKelpObjects, sizeof(float) * 3);
        initialRootPositionsBuffer.SetData(rootPositions);

        // Fill CPU kelp objects and node arrays
        kelpObjectsCPU = new KelpObject[totalKelpObjects];

        for (int kelpIndex = 0; kelpIndex < totalKelpObjects; kelpIndex++)
        {
            Vector3 baseLocal = rootPositions[kelpIndex]; // local-space base

            kelpObjectsCPU[kelpIndex].startStalkNodeIndex = kelpIndex * nodesPerStalk;
            kelpObjectsCPU[kelpIndex].stalkNodeCount = nodesPerStalk;
            kelpObjectsCPU[kelpIndex].startLeafNodeIndex = kelpIndex * leavesPerStalk;
            kelpObjectsCPU[kelpIndex].leafNodeCount = leavesPerStalk;

            // boundsCenter is stored relative to transform (so Update() can do transform.position + boundsCenter)
            Vector3 centerLocal = baseLocal + Vector3.up * (nodesPerStalk * segmentSpacing * 0.5f);
            Vector3 extents = new Vector3(0.5f, nodesPerStalk * segmentSpacing * 0.5f, 0.5f);

            kelpObjectsCPU[kelpIndex].boundsCenter = centerLocal;
            kelpObjectsCPU[kelpIndex].boundsExtents = extents;

            // initialize each node in local space (root + vertical offsets)
            for (int i = 0; i < nodesPerStalk; i++)
            {
                int nodeIndex = kelpObjectsCPU[kelpIndex].startStalkNodeIndex + i;
                if (nodeIndex >= totalStalkNodes) break; // safety

                Vector3 nodePosLocal = baseLocal + Vector3.up * (i * segmentSpacing);

                stalkNodes[nodeIndex].currentPos = nodePosLocal;
                stalkNodes[nodeIndex].previousPos = nodePosLocal;
                stalkNodes[nodeIndex].direction = Vector3.up;
                stalkNodes[nodeIndex].color = kelpColor;
                stalkNodes[nodeIndex].bendAmount = 0f;
                stalkNodes[nodeIndex].isTip = (i == nodesPerStalk - 1) ? 1 : 0;
            }

            // leaves
            for (int l = 0; l < leavesPerStalk; l++)
            {
                int leafObjIndex = kelpObjectsCPU[kelpIndex].startLeafNodeIndex + l;
                if (leafObjIndex >= totalLeafObjects) break;

                leafObjects[leafObjIndex].stalkNodeIndex =
                    kelpObjectsCPU[kelpIndex].startStalkNodeIndex + (l % nodesPerStalk);
                leafObjects[leafObjIndex].orientation = new Vector4(0, 0, 0, 1);
                leafObjects[leafObjIndex].bendValue = 0f;
                leafObjects[leafObjIndex].padding = 0;
            }
        }

        stalkNodesBuffer.SetData(stalkNodes);
        leafNodesBuffer.SetData(leafNodes);
        leafObjectsBuffer.SetData(leafObjects);
        kelpObjectsBuffer.SetData(kelpObjectsCPU);
    }

    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        // set simulation params
        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);
        kelpComputeShader.SetFloat("_SegmentSpacing", segmentSpacing);
        kelpComputeShader.SetFloat("_Time", Time.time);
        kelpComputeShader.SetFloat("_WindStrength", windStrength);
        kelpComputeShader.SetFloat("_WindFrequency", windFrequency);

        // nodes per stalk (make sure to set this BEFORE dispatching)
        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        kelpComputeShader.SetInt("_NodesPerStalk", nodesPerStalk);

        // bind buffers for verlet kernel
        kelpComputeShader.SetBuffer(verletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafNodesBuffer", leafNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafObjectsBuffer", leafObjectsBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_KelpObjectsBuffer", kelpObjectsBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "initialRootPositions", initialRootPositionsBuffer);

        // bind for constraint kernel too (it uses nodes and _NodesPerStalk)
        kelpComputeShader.SetBuffer(constraintKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(constraintKernel, "initialRootPositions", initialRootPositionsBuffer);

        int threadGroups = Mathf.CeilToInt(totalStalkNodes / 64f);
        kelpComputeShader.Dispatch(verletKernel, threadGroups, 1, 1);

        // relaxation iterations
        for (int i = 0; i < 15; i++)
            kelpComputeShader.Dispatch(constraintKernel, threadGroups, 1, 1); 

        // Frustum Culling
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        int visibleStalkCount = totalStalkNodes; 

        for (int i = 0; i < totalKelpObjects; i++)
        {
            Vector3 worldCenter = transform.position + kelpObjectsCPU[i].boundsCenter;
            Bounds bounds = new Bounds(worldCenter, kelpObjectsCPU[i].boundsExtents * 2f);

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
            {
                visibleStalkCount += kelpObjectsCPU[i].stalkNodeCount;
            }
        }

        // Rendering
        kelpRenderMaterial.SetVector("_WorldOffset", transform.position);
        kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
        kelpRenderMaterial.SetBuffer("_LeafNodesBuffer", leafNodesBuffer);
        kelpRenderMaterial.SetBuffer("_LeafObjectsBuffer", leafObjectsBuffer);
        kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

        Bounds drawBounds = new Bounds(
        transform.position + Vector3.up * (totalStalkNodes * segmentSpacing * 0.5f),
        new Vector3(spreadRadius * 2f + 10f, totalStalkNodes * segmentSpacing + 10f, spreadRadius * 2f + 10f)
    );

        // For now draw all nodes (simple and safe). Later we can implement proper per-stalk culling.
        int instanceCount = totalStalkNodes;

        Graphics.DrawMeshInstancedProcedural(
            kelpSegmentMesh,
            0,
            kelpRenderMaterial,
            drawBounds,
            instanceCount
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