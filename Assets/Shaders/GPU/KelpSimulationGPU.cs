using UnityEngine;
using System.Runtime.InteropServices;

public class KelpSimulationGPU_HDRP : MonoBehaviour
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
    public float segmentSpacing = 1f;
    public Color kelpColor = Color.green;
    public Color leafColor = new Color(0.2f, 0.8f, 0.2f, 1f);
    public float windStrength = 0.5f;
    public float windFrequency = 1f;

    [Range(2, 6)]
    public int leafNodesPerLeaf = 3;

    [Header("Placement")]
    public Terrain terrain;
    public LayerMask groundMask;
    public float spreadRadius = 5f;
    public float raycastHeight = 50f;

    [Header("Simulation")]
    [Range(1, 12)]
    public int constraintIterations = 4;

    float leafLength = 7f;

    [SerializeField] private bool debugMode = false; // Toggle in inspector 

    // Compute buffers
    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafSegmentsBuffer;
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;
    ComputeBuffer initialRootPositionsBuffer;

    // Dynamic colliders
    ComputeBuffer sphereCollidersBuffer;
    public Transform[] dynamicColliders;
    public float[] dynamicCollidersRadius;
    GPUSphereCollider[] collidersCPU;

    // Kernels
    int stalkKernel;
    int leafKernel;

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

    [StructLayout(LayoutKind.Sequential)]
    struct LeafObject
    {
        public Vector4 orientation;
        public Vector3 bendAxis;
        public float bendAngle;
        public int stalkNodeIndex;
        public float angleAroundStem;
        public Vector2 pad;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KelpObject
    {
        public int startStalkNodeIndex;
        public int stalkNodeCount;
        public int startLeafIndex;
        public int leafCount;
        public Vector3 boundsCenter; private float pad0;
        public Vector3 boundsExtents; private float pad1;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct GPUSphereCollider
    {
        public Vector3 position;
        public float radius;
    }

    void Start()
    {
        // Create material instances so we can safely set buffers/properties on them
        if (kelpRenderMaterial != null)
            kelpRenderMaterial = new Material(kelpRenderMaterial);
        if (leafRenderMaterial != null)
            leafRenderMaterial = new Material(leafRenderMaterial);

        InitializeBuffers();
        InitializeData();
        InitializeColliders();

        stalkKernel = kelpComputeShader.FindKernel("CS_StalkUpdate");
        leafKernel = kelpComputeShader.FindKernel("CS_LeafUpdate");

        if (targetCamera == null) targetCamera = Camera.main;

        if (kelpRenderMaterial != null) kelpRenderMaterial.enableInstancing = true;
        if (leafRenderMaterial != null) leafRenderMaterial.enableInstancing = true;
    }

    void InitializeBuffers()
    {
        stalkNodesBuffer?.Release();
        leafSegmentsBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();

        stalkNodesBuffer = new ComputeBuffer(totalStalkNodes, Marshal.SizeOf(typeof(StalkNode)));
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

        Vector3[] rootPositions = new Vector3[totalKelpObjects];

        for (int i = 0; i < totalKelpObjects; i++)
        {
            float t = i + 0.5f;
            float angle = t * 137.508f * Mathf.Deg2Rad;
            float r = Mathf.Sqrt(t / totalKelpObjects) * spreadRadius;

            float x = Mathf.Cos(angle) * r;
            float z = Mathf.Sin(angle) * r;

            float y = 0f;
            Vector3 rayOrigin = new Vector3(x + transform.position.x, raycastHeight, z + transform.position.z);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundMask))
                y = hit.point.y;
            else if (terrain != null)
                y = terrain.SampleHeight(new Vector3(x + transform.position.x, 0, z + transform.position.z)) + terrain.GetPosition().y;

            rootPositions[i] = new Vector3(x, y - transform.position.y, z); // local space
        }

        for (int k = 0; k < totalKelpObjects; k++)
        {
            Vector3 baseLocal = rootPositions[k];

            kelpObjectsCPU[k].startStalkNodeIndex = k * nodesPerStalk;
            kelpObjectsCPU[k].stalkNodeCount = nodesPerStalk;
            kelpObjectsCPU[k].startLeafIndex = k * leavesPerStalk;
            kelpObjectsCPU[k].leafCount = leavesPerStalk;

            kelpObjectsCPU[k].boundsCenter = baseLocal + Vector3.up * (nodesPerStalk * segmentSpacing * 0.5f);
            kelpObjectsCPU[k].boundsExtents = new Vector3(0.5f, nodesPerStalk * segmentSpacing * 0.5f, 0.5f);

            if (float.IsNaN(rootPositions[k].y)) continue;

            for (int i = 0; i < nodesPerStalk; i++)
            {
                int nodeIndex = kelpObjectsCPU[k].startStalkNodeIndex + i;
                if (nodeIndex >= totalStalkNodes) break;

                Vector3 nodePosLocal = baseLocal + Vector3.up * (i * segmentSpacing);
                stalkNodes[nodeIndex].currentPos = nodePosLocal;
                stalkNodes[nodeIndex].previousPos = nodePosLocal;
                stalkNodes[nodeIndex].direction = Vector3.up;
                stalkNodes[nodeIndex].color = kelpColor.linear;
                stalkNodes[nodeIndex].bendAmount = 0f;
                stalkNodes[nodeIndex].isTip = (i == nodesPerStalk - 1) ? 1 : 0;
            }

            for (int l = 0; l < leavesPerStalk; l++)
            {
                int li = kelpObjectsCPU[k].startLeafIndex + l;
                if (li >= totalLeafObjects) break;

                int minStartNode = 5;
                int maxEndNode = nodesPerStalk - 2;
                int stalkRange = Mathf.Max(1, maxEndNode - minStartNode);
                int leafStalkNode = minStartNode + Mathf.FloorToInt(((float)l / Mathf.Max(1, leavesPerStalk - 1)) * stalkRange);
                leafStalkNode = Mathf.Clamp(leafStalkNode, minStartNode, maxEndNode);

                leafObjs[li].stalkNodeIndex = kelpObjectsCPU[k].startStalkNodeIndex + leafStalkNode;
                leafObjs[li].angleAroundStem = Random.Range(0f, Mathf.PI * 2f);
                leafObjs[li].orientation = new Vector4(0, 0, 0, 1);
                leafObjs[li].bendAxis = Vector3.forward;
                leafObjs[li].bendAngle = 0f;
                leafObjs[li].pad = Vector2.zero;

                Vector3 n0Pos = stalkNodes[leafObjs[li].stalkNodeIndex].currentPos;
                Vector3 stalkDir = Vector3.up;
                if (leafObjs[li].stalkNodeIndex + 1 < kelpObjectsCPU[k].startStalkNodeIndex + nodesPerStalk)
                    stalkDir = (stalkNodes[leafObjs[li].stalkNodeIndex + 1].currentPos - n0Pos).normalized;

                if (stalkDir == Vector3.zero) stalkDir = Vector3.up;

                float stalkTwist = Random.Range(0f, Mathf.PI * 2f);
                Vector3 tmpUp = Mathf.Abs(stalkDir.y) > 0.95f ? Vector3.right : Vector3.up;
                Vector3 side = Vector3.Normalize(Vector3.Cross(tmpUp, stalkDir));
                Vector3 bin = Vector3.Normalize(Vector3.Cross(stalkDir, side));
                float ct = Mathf.Cos(leafObjs[li].angleAroundStem);
                float sa = Mathf.Sin(leafObjs[li].angleAroundStem);
                Vector3 around = (side * ct + bin * sa).normalized;
                Vector3 outward = around * 0.02f;

                float step = leafLength / (leafNodesPerLeaf - 1);
                for (int n = 0; n < leafNodesPerLeaf; n++)
                {
                    int segIndex = li * leafNodesPerLeaf + n;
                    Vector3 p = n0Pos + outward + stalkDir * step * n;
                    leafSegments[segIndex].currentPos = p;
                    leafSegments[segIndex].previousPos = p;
                    leafSegments[segIndex].color = leafColor.linear;
                }
            }
        }

        stalkNodesBuffer.SetData(stalkNodes);
        leafSegmentsBuffer.SetData(leafSegments);
        leafObjectsBuffer.SetData(leafObjs);
        kelpObjectsBuffer.SetData(kelpObjectsCPU);

        initialRootPositionsBuffer?.Release();
        initialRootPositionsBuffer = new ComputeBuffer(totalKelpObjects, sizeof(float) * 3);
        initialRootPositionsBuffer.SetData(rootPositions);
    }

    void InitializeColliders()
    {
        int colliderCount = dynamicColliders != null ? dynamicColliders.Length : 0;
        collidersCPU = new GPUSphereCollider[colliderCount];

        sphereCollidersBuffer?.Release();
        if (colliderCount > 0)
            sphereCollidersBuffer = new ComputeBuffer(colliderCount, Marshal.SizeOf(typeof(GPUSphereCollider)));
    }

    void Update()
    {
        if (targetCamera == null) targetCamera = Camera.main;

        // --- Set globals for compute ---
        kelpComputeShader.SetFloat("_DeltaTime", Time.deltaTime);
        kelpComputeShader.SetVector("_Gravity", gravityForce);
        kelpComputeShader.SetFloat("_Damping", damping);
        kelpComputeShader.SetFloat("_SegmentSpacing", segmentSpacing);
        kelpComputeShader.SetFloat("_Time", Time.time);
        kelpComputeShader.SetFloat("_WindStrength", windStrength);
        kelpComputeShader.SetFloat("_WindFrequency", windFrequency);
        kelpComputeShader.SetFloat("_StemRadius", 0.05f);

        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);
        kelpComputeShader.SetInt("_NodesPerStalk", nodesPerStalk);
        kelpComputeShader.SetInt("_LeafNodesPerLeaf", Mathf.Max(2, leafNodesPerLeaf));
        kelpComputeShader.SetInt("_TotalLeafObjects", totalLeafObjects);

        // --- Collider updates (only if present) ---
        if (dynamicColliders != null && dynamicColliders.Length > 0)
        {
            for (int i = 0; i < dynamicColliders.Length; i++)
            {
                if (dynamicColliders[i] == null) continue;
                collidersCPU[i].position = dynamicColliders[i].position - transform.position;
                collidersCPU[i].radius = dynamicCollidersRadius[i];
            }
            sphereCollidersBuffer.SetData(collidersCPU);
        }

        // --- Bind compute buffers to kernels ---
        kelpComputeShader.SetBuffer(stalkKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(stalkKernel, "initialRootPositions", initialRootPositionsBuffer);
        kelpComputeShader.SetBuffer(stalkKernel, "_SphereColliders", sphereCollidersBuffer);
        kelpComputeShader.SetInt("_ColliderCount", dynamicColliders != null ? dynamicColliders.Length : 0);

        kelpComputeShader.SetBuffer(leafKernel, "_StalkNodesBuffer", stalkNodesBuffer);
        kelpComputeShader.SetBuffer(leafKernel, "_LeafSegmentsBuffer", leafSegmentsBuffer);
        kelpComputeShader.SetBuffer(leafKernel, "_LeafObjectsBuffer", leafObjectsBuffer);

        // --- Dispatch compute ---
        int stalkGroups = Mathf.Max(1, Mathf.CeilToInt(totalStalkNodes / 64f));
        int leafGroups = Mathf.Max(1, Mathf.CeilToInt(totalLeafObjects / 64f));

        for (int i = 0; i < constraintIterations; i++)
            kelpComputeShader.Dispatch(stalkKernel, stalkGroups, 1, 1);

        kelpComputeShader.Dispatch(leafKernel, leafGroups, 1, 1);

        // --- Draw call ---
        if (kelpRenderMaterial != null)
        {
            // important: set buffers & flags on the material instance
            kelpRenderMaterial.SetColor("_BaseColor", kelpColor.linear);
            kelpRenderMaterial.SetVector("_WorldOffset", transform.position);
            kelpRenderMaterial.SetBuffer("_StalkNodesBuffer", stalkNodesBuffer);
            kelpRenderMaterial.SetBuffer("_KelpObjectsBuffer", kelpObjectsBuffer);

            // tell the shader to use instanced transform path
            kelpRenderMaterial.SetFloat("_Kelp_UseInstance", 1.0f);
        }

        if (leafRenderMaterial != null)
        {
            leafRenderMaterial.SetColor("_BaseColor", leafColor.linear);
            leafRenderMaterial.SetVector("_WorldOffset", transform.position);
            leafRenderMaterial.SetInt("_LeafNodesPerLeaf", Mathf.Max(2, leafNodesPerLeaf));
            leafRenderMaterial.SetBuffer("_LeafSegmentsBuffer", leafSegmentsBuffer);
            leafRenderMaterial.SetBuffer("_LeafObjectsBuffer", leafObjectsBuffer);
            leafRenderMaterial.SetFloat("_Kelp_UseInstance", 1.0f);
        }

        // ensure bounds are generous to avoid culling issues
        Bounds drawBounds = GetDrawBounds();

        Graphics.DrawMeshInstancedProcedural(
            kelpSegmentMesh, 0, kelpRenderMaterial, drawBounds, totalStalkNodes
        );
        Graphics.DrawMeshInstancedProcedural(
            kelpLeafMesh, 0, leafRenderMaterial, drawBounds, totalLeafObjects
        );

        // (Optional) reset the flag if you rely on the same material elsewhere; we keep it set here
        // kelpRenderMaterial.SetFloat("_Kelp_UseInstance", 0f);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!debugMode || stalkNodesBuffer == null || kelpObjectsCPU == null)
            return;

        // --- Draw stalk nodes ---
        int nodesToDraw = Mathf.Min(500, stalkNodesBuffer.count);
        StalkNode[] nodes = new StalkNode[nodesToDraw];
        stalkNodesBuffer.GetData(nodes, 0, 0, nodesToDraw);

        Gizmos.color = Color.green;
        for (int i = 0; i < nodesToDraw; i++)
        {
            Vector3 worldPos = transform.position + nodes[i].currentPos;
            Gizmos.DrawSphere(worldPos, 0.05f);

            if (i + 1 < nodesToDraw && nodes[i].isTip == 0)
            {
                Vector3 nextWorldPos = transform.position + nodes[i + 1].currentPos;
                Gizmos.DrawLine(worldPos, nextWorldPos);
            }
        }

        // --- Draw leaf segments ---
        if (leafSegmentsBuffer != null && leafObjectsBuffer != null)
        {
            int leafSegCount = leafSegmentsBuffer.count;
            LeafSegment[] leafSegs = new LeafSegment[leafSegCount];
            leafSegmentsBuffer.GetData(leafSegs, 0, 0, leafSegCount);

            LeafObject[] leafObjs = new LeafObject[totalLeafObjects];
            leafObjectsBuffer.GetData(leafObjs, 0, 0, totalLeafObjects);

            Gizmos.color = Color.cyan;

            for (int li = 0; li < totalLeafObjects; li++)
            {
                int startIndex = li * leafNodesPerLeaf;
                int endIndex = Mathf.Min(startIndex + leafNodesPerLeaf, leafSegCount);

                Vector3 prevPos = transform.position + leafSegs[startIndex].currentPos;

                // Draw first sphere
                Gizmos.DrawSphere(prevPos, 0.03f);

                for (int n = startIndex + 1; n < endIndex; n++)
                {
                    Vector3 curPos = transform.position + leafSegs[n].currentPos;
                    Gizmos.DrawLine(prevPos, curPos); // connect leaf nodes
                    Gizmos.DrawSphere(curPos, 0.03f); // draw leaf node
                    prevPos = curPos;
                }
            }
        }
    }
#endif

    Bounds GetDrawBounds()
    {
        // enlarge bounds to be safe from culling issues
        float maxHeight = totalStalkNodes * segmentSpacing + leafLength; // include leaf extension
        float radius = spreadRadius + 5f; // bigger margin
        return new Bounds(
            transform.position + Vector3.up * maxHeight * 0.5f,
            new Vector3(radius * 2f, maxHeight, radius * 2f)
        );
    }

    void OnDestroy()
    {
        stalkNodesBuffer?.Release();
        leafSegmentsBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();
        sphereCollidersBuffer?.Release();
    }
}