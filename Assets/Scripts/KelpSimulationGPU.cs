using UnityEngine;
using System.Runtime.InteropServices;

public class KelpSimulationGPU_Advanced : MonoBehaviour
{
    // === Settings ===
    [Header("Kelp Settings")]
    public int totalStalkNodes = 1000;
    public int totalLeafNodes = 2000;
    public int totalLeafObjects = 500;
    public int totalKelpObjects = 50;

    [Header("Physics")]
    public Vector3 gravityForce = new Vector3(0, -9.81f, 0);
    public float damping = 0.98f;
    public float deltaTime = 0.02f;

    [Header("References")]
    public ComputeShader kelpComputeShader;
    public Material kelpRenderMaterial;
    public Mesh kelpSegmentMesh;

    // === Compute buffers ===
    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafNodesBuffer;
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;

    // Kernel index
    int verletKernel;

    // Struct definitions matching GPU side
    [StructLayout(LayoutKind.Sequential)]
    struct StalkNode
    {
        public Vector3 currentPos;
        public Vector3 previousPos;
        public Vector3 direction;
        public Vector4 color;
        public float bendAmount;
        // padding to 48 bytes total if needed
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LeafNode
    {
        public Vector3 currentPos;
        public Vector3 previousPos;
        public Vector4 color;
        // other leaf data here
    }

    [StructLayout(LayoutKind.Sequential)]
    struct LeafObject
    {
        public Vector4 orientation; // quaternion or Euler
        public float bendValue;
        public int stalkNodeIndex;
        // padding if needed
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KelpObject
    {
        public int startStalkNodeIndex;
        public int stalkNodeCount;
        public int startLeafNodeIndex;
        public int leafNodeCount;
        // type, color etc can be added here
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
        // Initialize stalk nodes with starting positions in a column
        StalkNode[] stalkNodes = new StalkNode[totalStalkNodes];
        for (int i = 0; i < totalStalkNodes; i++)
        {
            Vector3 pos = transform.position + Vector3.up * (i * 0.2f);
            stalkNodes[i].currentPos = pos;
            stalkNodes[i].previousPos = pos;
            stalkNodes[i].direction = Vector3.up;
            stalkNodes[i].color = new Vector4(0, 1, 0, 1);
            stalkNodes[i].bendAmount = 0f;
        }
        stalkNodesBuffer.SetData(stalkNodes);

        // Initialize leaf nodes, leaf objects, and kelp objects similarly...
        // For brevity, initialize empty arrays here:
        leafNodesBuffer.SetData(new LeafNode[totalLeafNodes]);
        leafObjectsBuffer.SetData(new LeafObject[totalLeafObjects]);
        kelpObjectsBuffer.SetData(new KelpObject[totalKelpObjects]);
    }

    void Update()
    {
        // Dispatch the compute shader to update physics on GPU
        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);

        kelpComputeShader.SetBuffer(verletKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafNodesBuffer", leafNodesBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_LeafObjectsBuffer", leafObjectsBuffer);
        kelpComputeShader.SetBuffer(verletKernel, "_KelpObjectsBuffer", kelpObjectsBuffer);

        int threadGroups = Mathf.CeilToInt(totalStalkNodes / 64f);
        kelpComputeShader.Dispatch(verletKernel, threadGroups, 1, 1);

        // Set buffers on render material
        kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
        kelpRenderMaterial.SetBuffer("_LeafNodesBuffer", leafNodesBuffer);
        kelpRenderMaterial.SetBuffer("_LeafObjectsBuffer", leafObjectsBuffer);
        kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

        // Draw the kelp stalk mesh instanced procedurally
        Graphics.DrawMeshInstancedProcedural(
            kelpSegmentMesh,
            0,
            kelpRenderMaterial,
            new Bounds(transform.position, Vector3.one * 100f),
            totalStalkNodes
        );
    }

    void OnDestroy()
    {
        stalkNodesBuffer?.Release();
        leafNodesBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release(); 
    } 
} 