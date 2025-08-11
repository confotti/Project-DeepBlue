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
        StalkNode[] stalkNodes = new StalkNode[totalStalkNodes];
        LeafNode[] leafNodes = new LeafNode[totalLeafNodes];
        LeafObject[] leafObjects = new LeafObject[totalLeafObjects];

        int nodesPerStalk = totalStalkNodes / totalKelpObjects;
        int leavesPerStalk = totalLeafObjects / totalKelpObjects;

        Vector3[] rootPositions = new Vector3[totalKelpObjects];

        for (int i = 0; i < totalKelpObjects; i++)
        {
            rootPositions[i] = transform.position + new Vector3(i * 0.5f, 0, 0);
        }

        if (initialRootPositionsBuffer != null)
            initialRootPositionsBuffer.Release();

        initialRootPositionsBuffer = new ComputeBuffer(totalKelpObjects, sizeof(float) * 3);
        initialRootPositionsBuffer.SetData(rootPositions); 

        for (int kelpIndex = 0; kelpIndex < totalKelpObjects; kelpIndex++)
        {
            Vector3 basePosition = transform.position + new Vector3(kelpIndex * 0.5f, 0, 0);

            kelpObjectsCPU[kelpIndex].startStalkNodeIndex = kelpIndex * nodesPerStalk;
            kelpObjectsCPU[kelpIndex].stalkNodeCount = nodesPerStalk;
            kelpObjectsCPU[kelpIndex].startLeafNodeIndex = kelpIndex * leavesPerStalk;
            kelpObjectsCPU[kelpIndex].leafNodeCount = leavesPerStalk;

            Vector3 center = basePosition + Vector3.up * (nodesPerStalk * segmentSpacing * 0.5f);
            Vector3 extents = new Vector3(0.5f, nodesPerStalk * segmentSpacing * 0.5f, 0.5f);

            kelpObjectsCPU[kelpIndex].boundsCenter = center - transform.position;
            kelpObjectsCPU[kelpIndex].boundsExtents = extents;

            for (int i = 0; i < nodesPerStalk; i++)
            {
                int nodeIndex = kelpObjectsCPU[kelpIndex].startStalkNodeIndex + i;
                Vector3 nodePos = new Vector3(0, i * segmentSpacing, 0);

                stalkNodes[nodeIndex].currentPos = nodePos;
                stalkNodes[nodeIndex].previousPos = nodePos;
                stalkNodes[nodeIndex].direction = Vector3.up;
                stalkNodes[nodeIndex].color = kelpColor;
                stalkNodes[nodeIndex].bendAmount = 0f;
                stalkNodes[nodeIndex].isTip = (i == nodesPerStalk - 1) ? 1 : 0;

                Debug.Log($"Kelp {kelpIndex}, Node {i}: Pos {nodePos}");
            } 

            for (int l = 0; l < leavesPerStalk; l++)
            {
                int leafObjIndex = kelpObjectsCPU[kelpIndex].startLeafNodeIndex + l;

                leafObjects[leafObjIndex].stalkNodeIndex = kelpObjectsCPU[kelpIndex].startStalkNodeIndex + (l % nodesPerStalk);
                leafObjects[leafObjIndex].orientation = new Vector4(0, 0, 0, 1); // Identity quaternion
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
        if (targetCamera == null)
            targetCamera = Camera.main;

        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);
        kelpComputeShader.SetFloat("_SegmentSpacing", segmentSpacing);
        kelpComputeShader.SetFloat("_Time", Time.time);
        kelpComputeShader.SetFloat("_WindStrength", windStrength);
        kelpComputeShader.SetFloat("_WindFrequency", windFrequency);

        kelpComputeShader.SetBuffer(verletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafNodesBuffer", leafNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafObjectsBuffer", leafObjectsBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_KelpObjectsBuffer", kelpObjectsBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "initialRootPositions", initialRootPositionsBuffer);
        kelpComputeShader.SetBuffer(constraintKernel, "initialRootPositions", initialRootPositionsBuffer);

        int threadGroups = Mathf.CeilToInt(totalStalkNodes / 64f);
        kelpComputeShader.Dispatch(verletKernel, threadGroups, 1, 1);

        kelpComputeShader.SetFloat("_SegmentSpacing", segmentSpacing);
        kelpComputeShader.SetBuffer(constraintKernel, "_StalkNodesBuffer", stalkNodesBuffer);

        for (int i = 0; i < 15; i++) 
        {
            kelpComputeShader.Dispatch(constraintKernel, threadGroups, 1, 1);
        }

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
            new Vector3(10f, totalStalkNodes * segmentSpacing, 10f)
        );

        if (visibleStalkCount > 0)
        {
            Graphics.DrawMeshInstancedProcedural(
                kelpSegmentMesh,
                0,
                kelpRenderMaterial,
                drawBounds,
                visibleStalkCount
            );
        }
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