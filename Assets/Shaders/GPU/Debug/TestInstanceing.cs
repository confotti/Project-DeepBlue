using UnityEngine;

public class TestInstancing : MonoBehaviour
{
    public Material testMaterial;
    public Mesh testMesh;

    private ComputeBuffer testBuffer;

    void Start()
    {
        // Define 1 test position
        Vector3[] positions = new Vector3[1];
        positions[0] = Vector3.zero;

        // Create and assign the StructuredBuffer
        testBuffer = new ComputeBuffer(positions.Length, sizeof(float) * 3);
        testBuffer.SetData(positions);
        testMaterial.SetBuffer("_TestPositions", testBuffer);
    }

    void Update()
    {
        // Draw one instanced mesh at the test position
        Graphics.DrawMeshInstancedProcedural(
            testMesh,
            0,
            testMaterial,
            new Bounds(Vector3.zero, Vector3.one * 10f),
            1 // instance count
        );
    }

    void OnDestroy()
    {
        // Always release compute buffers!
        if (testBuffer != null)
        {
            testBuffer.Release(); 
        }
    }
}
