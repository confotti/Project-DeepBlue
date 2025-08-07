using UnityEngine;
using System.Collections.Generic;

public class BasicMeshCombineCompute : MonoBehaviour 
{
    [Header("Prefab Settings")]
    public GameObject[] values;          // The prefabs to randomly choose from
    public GameObject topPrefab;         // The special prefab to spawn last on top

    private void Start()
    {
        int countToSpawn = Random.Range(25, 36);  // 10 to 15 inclusive
        List<Vector3[]> allVertices = new List<Vector3[]>();

        float currentY = 0f;

        // === Spawn and stack main prefabs ===
        for (int i = 0; i < countToSpawn; i++)
        {
            GameObject prefabToSpawn = values[i % values.Length];
            GameObject newObj = Instantiate(prefabToSpawn, transform);

            float height = GetPrefabHeight(newObj);
            newObj.transform.position = transform.position + new Vector3(0f, currentY + height / 2f, 0f);
            currentY += height; 

            AddMeshWorldVertices(newObj, allVertices);
        }

        // === Spawn and place the top prefab ===
        if (topPrefab != null)
        {
            GameObject topObj = Instantiate(topPrefab, transform);

            float topHeight = GetPrefabHeight(topObj);
            topObj.transform.position = transform.position + new Vector3(0f, currentY + topHeight / 2f, 0f);
            currentY += topHeight;

            AddMeshWorldVertices(topObj, allVertices);
        }

        List<Transform> segments = new List<Transform>();
        foreach (Transform child in transform)
        {
            segments.Add(child);
        }

        // Add animation script and set neighbors
        for (int i = 0; i < segments.Count; i++)
        {
            Transform seg = segments[i];

            var animator = seg.gameObject.AddComponent<KelpSegmentAnimator>();
            animator.Initialize(i);

            if (i > 0)
                animator.previousSegment = segments[i - 1];
            if (i < segments.Count - 1)
                animator.nextSegment = segments[i + 1];
        } 
    }

    private float GetPrefabHeight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }

        // If no renderer, fallback
        return 1f;
    }

    private void AddMeshWorldVertices(GameObject obj, List<Vector3[]> allVerts)
    {
        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf != null)
        {
            Mesh mesh = mf.sharedMesh;
            Vector3[] verts = mesh.vertices;

            for (int v = 0; v < verts.Length; v++)
            {
                verts[v] = obj.transform.TransformPoint(verts[v]);
            }

            allVerts.Add(verts);
        }
    }

    private Vector3[] FlattenVertexArrays(List<Vector3[]> arrays)
    {
        int total = 0;
        foreach (var arr in arrays)
            total += arr.Length;

        Vector3[] result = new Vector3[total];
        int offset = 0;
        foreach (var arr in arrays)
        {
            arr.CopyTo(result, offset);
            offset += arr.Length;
        }

        return result;
    }
}