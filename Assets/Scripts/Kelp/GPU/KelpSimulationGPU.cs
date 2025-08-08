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

    [Header("Visual Tuning")]
    public float segmentSpacing = 0.1f;

    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafNodesBuffer;
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;

    int verletKernel;

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
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KelpObject
    {
        public int startStalkNodeIndex;
        public int stalkNodeCount;
        public int startLeafNodeIndex;
        public int leafNodeCount;
    }

    void Start()
    {
        InitializeBuffers();
        InitializeData();
        verletKernel = kelpComputeShader.FindKernel("CS_VerletUpdate");
    }

    void InitializeBuffers()
    {
        stalkNodesBuffer = new ComputeBuffer(totalStalkNodes, Marshal.SizeOf(typeof(StalkNode)));
        leafNodesBuffer = new ComputeBuffer(totalLeafNodes, Marshal.SizeOf(typeof(LeafNode)));
        leafObjectsBuffer = new ComputeBuffer(totalLeafObjects, Marshal.SizeOf(typeof(LeafObject)));
        kelpObjectsBuffer = new ComputeBuffer(totalKelpObjects, Marshal.SizeOf(typeof(KelpObject)));
    }

    void InitializeData()
    {
        StalkNode[] stalkNodes = new StalkNode[totalStalkNodes];
        LeafNode[] leafNodes = new LeafNode[totalLeafNodes];
        LeafObject[] leafObjects = new LeafObject[totalLeafObjects];
        KelpObject[] kelpObjects = new KelpObject[totalKelpObjects];

        int nodesPerStalk = totalStalkNodes / totalKelpObjects;
        int leavesPerStalk = totalLeafObjects / totalKelpObjects;

        // Initialize stalk nodes in a vertical column with spacing on Y axis,
        // offset each kelp object in X to prevent overlap.
        for (int kelpIndex = 0; kelpIndex < totalKelpObjects; kelpIndex++)
        {
            Vector3 basePosition = transform.position + new Vector3(kelpIndex * 0.5f, 0, 0);

            // Setup kelp object range
            kelpObjects[kelpIndex].startStalkNodeIndex = kelpIndex * nodesPerStalk;
            kelpObjects[kelpIndex].stalkNodeCount = nodesPerStalk;
            kelpObjects[kelpIndex].startLeafNodeIndex = kelpIndex * leavesPerStalk;
            kelpObjects[kelpIndex].leafNodeCount = leavesPerStalk;

            for (int i = 0; i < nodesPerStalk; i++)
            {
                int nodeIndex = kelpObjects[kelpIndex].startStalkNodeIndex + i;

                Vector3 nodePos = basePosition + new Vector3(0, i * segmentSpacing, 0);

                stalkNodes[nodeIndex].currentPos = nodePos;
                stalkNodes[nodeIndex].previousPos = nodePos;
                stalkNodes[nodeIndex].direction = Vector3.up;
                stalkNodes[nodeIndex].color = Color.green;
                stalkNodes[nodeIndex].bendAmount = 0f;
                stalkNodes[nodeIndex].isTip = (i == nodesPerStalk - 1) ? 1 : 0;
            }

            // Initialize leaves to sit somewhere on stalk nodes
            for (int l = 0; l < leavesPerStalk; l++)
            {
                int leafObjIndex = kelpObjects[kelpIndex].startLeafNodeIndex + l;

                leafObjects[leafObjIndex].stalkNodeIndex = kelpObjects[kelpIndex].startStalkNodeIndex + (l % nodesPerStalk);
                leafObjects[leafObjIndex].orientation = new Vector4(0, 0, 0, 1); // Identity quaternion
                leafObjects[leafObjIndex].bendValue = 0f;
            }
        }

        stalkNodesBuffer.SetData(stalkNodes);
        leafNodesBuffer.SetData(leafNodes);
        leafObjectsBuffer.SetData(leafObjects);
        kelpObjectsBuffer.SetData(kelpObjects);
    }

    void Update()
    {
        // Update compute shader parameters
        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);

        kelpComputeShader.SetBuffer(verletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafNodesBuffer", leafNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafObjectsBuffer", leafObjectsBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_KelpObjectsBuffer", kelpObjectsBuffer);

        int threadGroups = Mathf.CeilToInt(totalStalkNodes / 64f);
        kelpComputeShader.Dispatch(verletKernel, threadGroups, 1, 1);

        // Pass the world offset so meshes draw relative to this GameObject’s position
        kelpRenderMaterial.SetVector("_WorldOffset", transform.position);

        kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
        kelpRenderMaterial.SetBuffer("_LeafNodesBuffer", leafNodesBuffer);
        kelpRenderMaterial.SetBuffer("_LeafObjectsBuffer", leafObjectsBuffer);
        kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

        // Define bounds centered at this GameObject to help culling
        Bounds drawBounds = new Bounds(
            transform.position + Vector3.up * (totalStalkNodes * segmentSpacing * 0.5f),
            new Vector3(10f, totalStalkNodes * segmentSpacing, 10f)
        );

        // Draw all stalk node instances procedurally
        Graphics.DrawMeshInstancedProcedural(
            kelpSegmentMesh,
            0,
            kelpRenderMaterial,
            drawBounds,
            totalStalkNodes
        );
    }

    void OnDestroy()
    {
        // Release compute buffers
        stalkNodesBuffer?.Release();
        leafNodesBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
    }
} 