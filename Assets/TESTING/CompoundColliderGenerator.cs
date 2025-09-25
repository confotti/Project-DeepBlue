using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CombinedMeshCollider : MonoBehaviour
{
    [Tooltip("If true, will also delete MeshColliders on children.")]
    public bool removeChildColliders = true;

    void Start()
    {
        BuildCombinedCollider();
    }

    public void BuildCombinedCollider()
    {
        // Get all MeshFilters in children
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        // Build combined mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // Add MeshCollider to parent
        MeshCollider mc = gameObject.GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = combinedMesh;
        mc.convex = false; // set to true if needed, but note convex hulls are approximate

        // Optionally remove colliders from children
        if (removeChildColliders)
        {
            foreach (MeshCollider childMc in GetComponentsInChildren<MeshCollider>())
            {
                if (childMc.gameObject != gameObject) // don’t delete the new parent collider
                    Destroy(childMc);
            }
        }

        Debug.Log("Combined mesh collider created for parent.");
    }
}