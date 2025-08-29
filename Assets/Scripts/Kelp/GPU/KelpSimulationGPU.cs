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
    public float segmentSpacing = 1f; 
    public Color kelpColor = Color.white;
    public float windStrength = 0.5f;
    public float windFrequency = 1f; 

    // 2–3 nodes per leaf gives nice rope-like motion
    [Range(2, 6)]
    public int leafNodesPerLeaf = 3;

    [Header("Placement")]
    public Terrain terrain;
    public LayerMask groundMask; // set to your seabed/rock layer
    public float spreadRadius = 5f;
    public float raycastHeight = 50f; // how high above to cast rays from

    float leafLength = 7f; 

    // compute buffers
    ComputeBuffer stalkNodesBuffer;
    ComputeBuffer leafSegmentsBuffer;   // totalLeafObjects * leafNodesPerLeaf
    ComputeBuffer leafObjectsBuffer;
    ComputeBuffer kelpObjectsBuffer;
    ComputeBuffer initialRootPositionsBuffer;

    // kernels
    int stalkVerletKernel;
    int stalkConstraintKernel;
    int leafVerletKernel;        
    int leafConstraintKernel;    
    int updateLeavesKernel;       

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
            // Radial spread
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = spreadRadius * Mathf.Sqrt(Random.value); 
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius; 

            // Minimal jitter to avoid perfect symmetry
            Vector3 jitter = new Vector3(
                Random.Range(-0.1f, 0.1f),
                0f,
                Random.Range(-0.1f, 0.1f)
            );
            x += jitter.x;
            z += jitter.z; 

            float worldX = transform.position.x + x;
            float worldZ = transform.position.z + z;

            float y = 0f;
            bool foundGround = false;

            // 1️⃣ Try raycast against meshes
            Vector3 rayOrigin = new Vector3(worldX, raycastHeight, worldZ);
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundMask))
            {
                y = hit.point.y;
                foundGround = true;
            }

            // 2️⃣ If no mesh hit, fall back to terrain
            if (!foundGround && terrain != null)
            {
                y = terrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrain.GetPosition().y;
            }

            float yLocal = y - transform.position.y - 0.8f;
            Vector3 rootPosLocal = new Vector3(x, yLocal, z);
            rootPositions[i] = rootPosLocal; 
        } 

        // Fill stalks + leaves
        for (int k = 0; k < totalKelpObjects; k++)
        {
            Vector3 baseLocal = rootPositions[k] + transform.position; 

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

                Vector3 nodePosLocal = rootPositions[k] + Vector3.up * (i * segmentSpacing); 
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
                int randSegLocal = Random.Range(3, nodesPerStalk - 2); // start from node index 3
                int n0 = kelpObjectsCPU[k].startStalkNodeIndex + randSegLocal;
                leafObjs[li].stalkNodeIndex = n0;

                // assign random radial placement around stem
                leafObjs[li].angleAroundStem = Random.Range(0f, Mathf.PI * 2f);
                leafObjs[li].orientation = new Vector4(0, 0, 0, 1);
                leafObjs[li].bendAxis = new Vector3(0, 0, 1);
                leafObjs[li].bendAngle = 0f;
                leafObjs[li].pad = Vector2.zero;

                // ---- MATCH INIT TO GPU CONSTRAINT BASIS ----
                Vector3 n0Pos = stalkNodes[n0].currentPos;

                // compute stalk tangent for local frame
                Vector3 stalkDir = Vector3.up;
                if (randSegLocal + 1 < nodesPerStalk)
                {
                    stalkDir = (stalkNodes[n0 + 1].currentPos - n0Pos).normalized;
                }
                if (stalkDir == Vector3.zero) stalkDir = Vector3.up;

                float stalkTwist = Random.Range(0f, Mathf.PI * 2f); 
                // side/bin like in shader
                Vector3 tmpUp = Mathf.Abs(stalkDir.y) > 0.95f ? Vector3.right : Vector3.up;
                Vector3 side = Vector3.Normalize(Vector3.Cross(tmpUp, stalkDir));
                Vector3 bin = Vector3.Normalize(Vector3.Cross(stalkDir, side));

                // rotate the side/bin basis by stalkTwist
                float ct = Mathf.Cos(stalkTwist);
                float st = Mathf.Sin(stalkTwist);

                Vector3 rotatedSide = side * ct + bin * st;
                Vector3 rotatedBin = -side * st + bin * ct; 

                float ca = Mathf.Cos(leafObjs[li].angleAroundStem);
                float sa = Mathf.Sin(leafObjs[li].angleAroundStem);
                Vector3 around = (side * ca + bin * sa).normalized;

                // outward offset from stem
                Vector3 outward = around * 0.02f;

                // ---- DISTRIBUTE LEAF SEGMENTS ALONG FULL MESH HEIGHT ----
                float step = leafLength / (leafNodesPerLeaf - 1); 
                for (int n = 0; n < leafNodesPerLeaf; n++)
                {
                    int segIndex = li * leafNodesPerLeaf + n;

                    // distribute nodes evenly from base to tip
                    float yOffset = step * n;

                    // position = stalk base + outward offset + upward offset
                    Vector3 p = n0Pos + outward + stalkDir * yOffset;

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
        for (int i = 0; i < 35; i++) kelpComputeShader.Dispatch(stalkConstraintKernel, stalkGroups, 1, 1); 

        // --- Dispatch leaves (verlet over segments, then constraints over segments, then per-leaf update)
        int leafSegGroups = Mathf.Max(1, Mathf.CeilToInt(totalLeafSegments / 64f));
        kelpComputeShader.Dispatch(leafVerletKernel, leafSegGroups, 1, 1);
        for (int i = 0; i < 20; i++) kelpComputeShader.Dispatch(leafConstraintKernel, leafSegGroups, 1, 1);

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

        // === Frustum Culling ===
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(targetCamera);
        int visibleStalkCount = 0;

        for (int i = 0; i < totalKelpObjects; i++)
        {
            Vector3 worldCenter = transform.position + kelpObjectsCPU[i].boundsCenter;
            Bounds bounds = new Bounds(worldCenter, kelpObjectsCPU[i].boundsExtents * 2f);

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, bounds))
            {
                visibleStalkCount += kelpObjectsCPU[i].stalkNodeCount;
            }
        } 

        // Draw
        Bounds drawBounds = new Bounds(
            transform.position + Vector3.up * (totalStalkNodes * segmentSpacing * 0.5f),
            new Vector3(spreadRadius * 2f + 10f, totalStalkNodes * segmentSpacing + 10f, spreadRadius * 2f + 10f)
        );

        Graphics.DrawMeshInstancedProcedural(kelpSegmentMesh, 0, kelpRenderMaterial, drawBounds, totalStalkNodes);
        Graphics.DrawMeshInstancedProcedural(kelpLeafMesh, 0, leafRenderMaterial, drawBounds, totalLeafObjects);
    }

    /*void OnDrawGizmos() 
    {
        if (stalkNodesBuffer == null || leafSegmentsBuffer == null)
            return;

        StalkNode[] stalkNodes = new StalkNode[totalStalkNodes];
        stalkNodesBuffer.GetData(stalkNodes);

        Gizmos.color = Color.yellow;
        int nodesPerStalk = Mathf.Max(1, totalStalkNodes / totalKelpObjects);

        for (int i = 0; i < totalStalkNodes; i++)
        {
            Vector3 pos = transform.position + stalkNodes[i].currentPos; // <-- add transform.position
            Gizmos.DrawSphere(pos, 0.015f);

            // Draw line to next stalk node in same kelp
            if (i < totalStalkNodes - 1 && (i + 1) / nodesPerStalk == i / nodesPerStalk)
            {
                Vector3 nextPos = transform.position + stalkNodes[i + 1].currentPos; // <-- add transform.position
                Gizmos.DrawLine(pos, nextPos);
            }
        }

        // --- Draw leaf nodes ---
        int nodesPerLeaf = Mathf.Max(2, leafNodesPerLeaf);
        int totalLeafSegs = totalLeafObjects * nodesPerLeaf;

        LeafSegment[] leafSegs = new LeafSegment[totalLeafSegs];
        leafSegmentsBuffer.GetData(leafSegs);

        Gizmos.color = Color.green;

        for (int leafIndex = 0; leafIndex < totalLeafObjects; leafIndex++)
        {
            int baseIndex = leafIndex * nodesPerLeaf;

            for (int i = 0; i < nodesPerLeaf; i++)
            {
                if (baseIndex + i >= leafSegs.Length) break;

                Vector3 pos = transform.position + leafSegs[baseIndex + i].currentPos; // <-- add transform.position
                Gizmos.DrawSphere(pos, 0.01f);

                if (i < nodesPerLeaf - 1 && baseIndex + i + 1 < leafSegs.Length)
                {
                    Vector3 nextPos = transform.position + leafSegs[baseIndex + i + 1].currentPos; // <-- add transform.position
                    Gizmos.DrawLine(pos, nextPos);
                }
            }
        }
    } */

    void OnDestroy()
    {
        stalkNodesBuffer?.Release();
        leafSegmentsBuffer?.Release();
        leafObjectsBuffer?.Release();
        kelpObjectsBuffer?.Release();
        initialRootPositionsBuffer?.Release();
    }
} 